/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Make the speeds and forces be individual to each surface type, so that you can slide on multiple? but that's unfair to both STs of different collisions being the same
//TODO: move the particles to always work?

namespace PrecisionSurfaceEffects
{
    public sealed partial class CollisionEffects : CollisionEffectsMaker, IOnOnCollisionStay
    {
        //Constants
        private const float EXTRA_SEARCH_THICKNESS = 0.01f;
        private const int MAX_PARTICLE_TYPE_COUNT = 10;



        //Fields
#if UNITY_EDITOR
        [Header("Debug")]
        [Space(30)]
        [TextArea(1, 10)]
        //[ReadOnly]
        public string currentFrictionDebug;
#endif

        [Header("Quality")]
        [Space(30)]
        [Tooltip("Non-convex MeshCollider submeshes")]
        public bool findMeshColliderSubmesh = true;

        [Header("Impacts")]
        [Space(30)]
        public float impactCooldown = 0.1f;
        [Space(5)]
        [UnityEngine.Serialization.FormerlySerializedAs("doImpactByImpulseChangeRate ")]
        public bool doImpactByForceChange = true;
        [UnityEngine.Serialization.FormerlySerializedAs("impulseChangeRateToImpact")]
        public float forceChangeToImpact = 10;

        [SeperatorLine]
        [Header("Sounds")]
        [Space(30)]
        public SurfaceSoundSet soundSet;

        [Header("Scaling")]
        [Space(5)]
        [Tooltip("To easily make the speed be local/relative")]
        public float speedMultiplier = 1;
        public float forceMultiplier = 1;
        public float totalVolumeMultiplier = 0.3f;
        public float totalPitchMultiplier = 1;

        [Header("Impact Sound")]
        [Space(5)]
        public Sound impactSound = new Sound();

        [Header("Friction Sound")]
        [Space(5)]
        public bool doFrictionSound = true;
        public FrictionSound frictionSound = new FrictionSound(); //[Tooltip("When the friction soundType changes, this can be used to play impactSound")]

        [Header("Vibration Sound")]
        [Space(5)]
        public bool doVibrationSound = true;
        public VibrationSound vibrationSound = new VibrationSound();

        [SeperatorLine]
        [Header("Particles")]
        [Space(30)]
        public ParticlesType particlesType = ParticlesType.ImpactAndFriction;
        public SurfaceParticleSet particleSet;
        public Particles particles = new Particles();

        [SerializeField]
        [HideInInspector]
        private Rigidbody rb;
        [SerializeField]
        [HideInInspector]
        private new Collider collider;
        private bool neededOnCollisionStay;

        private float impactCooldownT;
        private SurfaceOutputs outputs2 = new SurfaceOutputs();
        private SurfaceOutputs outputs3 = new SurfaceOutputs();
        private readonly SurfaceOutputs averageOutputs = new SurfaceOutputs(); // List<CollisionSound> collisionSounds = new List<CollisionSound>();
        private readonly List<int> givenSources = new List<int>();
        private float weightedSpeed;
        private float forceSum;
        private bool downShifted;

        private List<float> previousImpulses = new List<float>();
        private List<float> currentImpulses = new List<float>();

        public OnSurfaceCallback onEnterParticles; //should I remove these?
        public OnSurfaceCallback onEnterSound;

        private static readonly ContactPoint[] contacts = new ContactPoint[64];



        //Properties
        private bool NeedOnCollisionStay
        {
            get
            {
                bool wanted = particlesType == ParticlesType.ImpactAndFriction || doFrictionSound || doImpactByForceChange;
                bool canReceiveCallbacks = collider != null && (collider.attachedRigidbody == null || rb != null); //This is (still) correct right?
                return wanted && canReceiveCallbacks;
            }
        }



        //Methods
        public static SurfaceOutputs FillCollisionData(Collision c, bool findMeshColliderSubmesh, SurfaceData data)
        {
            if (findMeshColliderSubmesh && RaycastTestSubmesh(c, out RaycastHit rh, out Vector3 pos))
            {
                SurfaceData.outputs.Clear();
                data.AddSurfaceTypes(c.collider, pos, triangleIndex: rh.triangleIndex);
                return SurfaceData.outputs;
            }

            return data.GetCollisionSurfaceTypes(c, shareList: true);
        }
        private static bool RaycastTestSubmesh(Collision c, out RaycastHit rh, out Vector3 pos)
        {
            if (c.collider is MeshCollider mc && !mc.convex)
            {
                var contact = c.GetContact(0);
                pos = contact.point;
                var norm = contact.normal; //this better be normalized!

                float searchThickness = EXTRA_SEARCH_THICKNESS + Mathf.Abs(contact.separation);

                if (mc.Raycast(new Ray(pos + norm * searchThickness, -norm), out rh, Mathf.Infinity)) //searchThickness * 2
                {
#if UNITY_EDITOR
                    float debugSize = 3;
                    Debug.DrawLine(pos + norm * debugSize, pos - norm * debugSize, Color.white, 0);
#endif
                    return true;
                }
            }

            pos = default;
            rh = default;
            return false;
        }
        private void GetSurfaceTypeOutputs(Collision c, bool doSound, bool doParticle, out SurfaceOutputs soundOutputs, out SurfaceOutputs particleOutputs)
        {
            soundOutputs = particleOutputs = null;

            if (doSound || doParticle)
            {
                SurfaceOutputs GetFlipFlopOutputs()
                {
                    var temp = SurfaceData.outputs;
                    SurfaceData.outputs = outputs2;
                    outputs2 = temp;
                    return temp;
                }

                if (findMeshColliderSubmesh && RaycastTestSubmesh(c, out RaycastHit rh, out Vector3 pos))
                {
                    SurfaceOutputs GetOutputs(SurfaceData data)
                    {
                        SurfaceData.outputs.Clear();
                        data.AddSurfaceTypes(c.collider, pos, triangleIndex: rh.triangleIndex);
                        return GetFlipFlopOutputs();
                    }

                    if (doSound)
                        soundOutputs = GetOutputs(soundSet.data);
                    if (doParticle)
                        particleOutputs = GetOutputs(particleSet.data);

                    return;
                }

                if (doSound)
                {
                    soundSet.data.GetCollisionSurfaceTypes(c, shareList: true);
                    soundOutputs = GetFlipFlopOutputs();
                }

                if (doParticle)
                {
                    particleSet.data.GetCollisionSurfaceTypes(c, shareList: true);
                    particleOutputs = GetFlipFlopOutputs();
                }
            }
        }

        private bool Stop(Collision collision)
        {
            Transform target;
            if (collision.rigidbody != null)
                target = collision.rigidbody.transform;
            else
                target = collision.collider.transform;

            var otherCSM = target.GetComponent<CollisionEffectsMaker>();
            if (otherCSM != null)
            {
                if (otherCSM.priority == priority)
                    return (otherCSM.gameObject.GetInstanceID() > gameObject.GetInstanceID()) ^ (Time.frameCount % 2 == 0);

                return priority < otherCSM.priority;
            }
            return false;
        }

        private void DoFrictionSound(Collision collision, SurfaceOutputs outputs, float impulse, float speed)
        {
            var force = impulse / Time.deltaTime; //force = Mathf.Max(0, Mathf.Min(frictionSound.maxForce, force) - frictionSound.minForce);
            force *= frictionSound.SpeedFader(speed); //So that it is found the maximum with this in mind

            if (force > 0)
            {
                if (doVibrationSound)
                {
                    vibrationSound.Add(force * vibrationSound.frictionForceMultiplier, speed * vibrationSound.frictionSpeedMultiplier);
                }

                forceSum += force;
                weightedSpeed += force * speed;

                float influence = force / forceSum;
                float invInfluence = 1 - influence;

                for (int i = 0; i < outputs.Count; i++)
                {
                    var output = outputs[i];

                    bool success = false;
                    for (int ii = 0; ii < averageOutputs.Count; ii++)
                    {
                        var o = averageOutputs[ii];
                        if (o.surfaceTypeID == output.surfaceTypeID && o.particleOverrides == output.particleOverrides)
                        {
                            void Lerp(ref float from, float to)
                            {
                                from = invInfluence * from + influence * to;
                            }

                            Lerp(ref o.weight, output.weight);
                            Lerp(ref o.volumeMultiplier, output.volumeMultiplier);
                            Lerp(ref o.pitchMultiplier, output.pitchMultiplier);
                            Lerp(ref o.particleSizeMultiplier, output.particleSizeMultiplier);
                            Lerp(ref o.particleCountMultiplier, output.particleCountMultiplier);
                            o.color = invInfluence * o.color + influence * output.color;

                            averageOutputs[ii] = o;

                            success = true;
                            break;
                        }
                    }

                    if (!success)
                    {
                        averageOutputs.Add
                        (
                            new SurfaceOutput()
                            {
                                weight = output.weight * influence,

                                surfaceTypeID = output.surfaceTypeID,
                                volumeMultiplier = output.volumeMultiplier,
                                pitchMultiplier = output.pitchMultiplier,
                                particleSizeMultiplier = output.particleSizeMultiplier,
                                particleCountMultiplier = output.particleCountMultiplier,
                                color = output.color,
                            }
                        );
                    }
                }
            }
        }

        private void Calculate(Collision collision, out int contactCount, out Vector3 center, out Vector3 normal, out float radius, out Vector3 vel0, out Vector3 vel1, out float mass0, out float mass1)
        {
            radius = 0;

            contactCount = collision.GetContacts(contacts);
            if (contactCount == 1)
            {
                var contact = contacts[0];
                center = contact.point;
                normal = contact.normal;
            }
            else
            {
                normal = new Vector3();
                center = new Vector3();

                for (int i = 0; i < contactCount; i++)
                {
                    var contact = contacts[i];
                    normal += contact.normal;
                    center += contact.point;
                }

                normal.Normalize();
                float invCount = 1f / contactCount;
                center *= invCount;

                for (int i = 0; i < contactCount; i++)
                {
                    var contact = contacts[i];
                    radius += (contact.point - center).magnitude; //this doesn't care if it is lateral to normal, but should it?
                }

                radius *= invCount;
            }

            vel0 = Utility.GetVelocityMass(contacts[0].thisCollider.attachedRigidbody, center, out mass0);
            vel1 = Utility.GetVelocityMass(collision.rigidbody, center, out mass1);
        }

        private void DoParticles(Collision c, SurfaceOutputs particleOutputs, float dt, Vector3 center, Vector3 normal, float radius, Vector3 vel0, Vector3 vel1, float mass0, float mass1, float impulse, float speed)
        {
            if (particleOutputs.Count != 0) //particleSet != null && 
            {
                var rot = Quaternion.FromToRotation(Vector3.forward, normal); //Vector3.up

                particleOutputs.Downshift(MAX_PARTICLE_TYPE_COUNT, particles.minimumTypeWeight);

                float speedFader = particles.SpeedFader(speed);

                for (int i = 0; i < particleOutputs.Count; i++)
                {
                    var o = particleOutputs[i];

                    var sp = particleSet.GetSurfaceParticles(ref o);

                    if (sp != null)
                    {
                        sp.GetInstance().PlayParticles
                        (
                            o.color, o.particleCountMultiplier * particles.particleCountMultiplier, o.particleSizeMultiplier * particles.particleSizeMultiplier,
                            o.weight * speedFader,
                            impulse, speed,
                            rot, center, radius + particles.minimumParticleShapeRadius, particleOutputs.hitNormal,
                            vel0, vel1,
                            mass0, mass1,
                            dt
                        );
                    }
                }
            }
        }

        public void OnOnCollisionStay(Collision collision)
        {
            if (!isActiveAndEnabled)
                return;


            Vector3 impulseNormal = collision.impulse;
            float impulse = impulseNormal.magnitude;
            float absImpulse = impulse;
            if (impulse != 0)
                impulseNormal /= impulse;
            impulse *= forceMultiplier;


            bool stop = Stop(collision);


            //Impact By Impulse ChangeRate
            if (doImpactByForceChange)
            {
                currentImpulses.Add(impulse);

                float previousImpulse = 0;
                if (previousImpulses.Count >= currentImpulses.Count)
                    previousImpulse = previousImpulses[currentImpulses.Count - 1];

                if ((impulse - previousImpulse) / Time.deltaTime >= forceChangeToImpact)
                    OnCollisionEnter(collision); //previousImpulse = impulse;
            }


            if (stop)
                return;

            var doParticles = particlesType == ParticlesType.ImpactAndFriction;
            GetSurfaceTypeOutputs(collision, doFrictionSound, doParticles, out SurfaceOutputs soundOutputs, out SurfaceOutputs particleOutputs);



            //Calculation
            Calculate(collision, out int contactCount, out Vector3 center, out Vector3 normal, out float radius, out Vector3 vel0, out Vector3 vel1, out float mass0, out float mass1);

            float lateralSpeed = Vector3.ProjectOnPlane(vel1 - vel0, normal).magnitude;
            float frictionImpulser = Mathf.Lerp(1, frictionSound.frictionNormalForceMultiplier, Mathf.Abs(Vector3.Dot(impulseNormal, normal))); //I'm not sure if this works


            //Friction Sounds
            if (doFrictionSound)
                DoFrictionSound(collision, soundOutputs, frictionImpulser * impulse, speedMultiplier * lateralSpeed);


            //Particles
            if (doParticles)
                DoParticles(collision, particleOutputs, Time.deltaTime, center, normal, radius, vel0, vel1, mass0, mass1, frictionImpulser * absImpulse, lateralSpeed);
        }

        public void ResetSounds()
        {
            //?

            vibrationSound.force = 0;
            vibrationSound.weightedSpeed = 0;

            forceSum = 0;
            weightedSpeed = 0;
            downShifted = false;
            averageOutputs.Clear(); //?
        }



        //Datatypes
        public enum ParticlesType { None, ImpactOnly, ImpactAndFriction }

        public delegate void OnSurfaceCallback(Collision collision, SurfaceOutputs outputs);



        //Lifecycle
        private void Awake()
        {
            frictionSound.sources = new FrictionSound.Source[frictionSound.audioSources.Length];
            for (int i = 0; i < frictionSound.sources.Length; i++)
            {
                frictionSound.sources[i] = new FrictionSound.Source() { audioSource = frictionSound.audioSources[i] };
            }
        }

        private void OnEnable()
        {
            neededOnCollisionStay = NeedOnCollisionStay;
            if (neededOnCollisionStay)
                OnCollisionStayer.Add(gameObject, this);
        }
        private void OnDisable()
        {
            if (neededOnCollisionStay)
                OnCollisionStayer.Remove(gameObject, this);

            for (int i = 0; i < frictionSound.audioSources.Length; i++)
                frictionSound.audioSources[i].Pause();

            if (doVibrationSound)
                vibrationSound.audioSource.Pause();
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            rb = GetComponent<Rigidbody>();
            collider = GetComponent<Collider>();

            if (!Application.isPlaying)
                currentFrictionDebug = "(This only works in playmode)";

            impactSound.Validate(false);
            frictionSound.Validate(true);

            if (doVibrationSound)
            {
                vibrationSound.audioSource.loop = true;
                vibrationSound.audioSource.playOnAwake = false;
            }
        }
#endif

        private void FixedUpdate()
        {
            averageOutputs.Clear(); // collisionSounds.Clear();
            downShifted = false;
            weightedSpeed = 0;
            forceSum = 0;

            var temp = currentImpulses;
            currentImpulses = previousImpulses;
            previousImpulses = temp;
            currentImpulses.Clear();
        }

        internal void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled)
                return;


            //Impact Sound
            if (impactCooldownT <= 0)
            {
                //This prevents multiple sounds for one collision
                if (Stop(collision))
                    return;

                var absSpeed = collision.relativeVelocity.magnitude;
                var speed = speedMultiplier * absSpeed; //Can't consistently use CurrentRelativeVelocity(collision);, probably maybe because it's too late to get that speed (already resolved)
                var absImpulse = collision.impulse.magnitude;
                var impulse = forceMultiplier * absImpulse;
                float speedFade = impactSound.SpeedFader(speed);
                var vol = totalVolumeMultiplier * impactSound.Volume(impulse) * speedFade; //force //Here "force" is actually an impulse

                if (vol > 0.00000000000001f)
                {
                    if (doVibrationSound)
                    {
                        vibrationSound.Add(impulse * vibrationSound.impactForceMultiplier * speedFade, speed * vibrationSound.impactSpeedMultiplier); //Remove speedFade?
                    }

                    impactCooldownT = impactCooldown;

                    bool doParticles = particlesType != ParticlesType.None;
                    GetSurfaceTypeOutputs(collision, true, doParticles, out SurfaceOutputs soundOutputs, out SurfaceOutputs particleOutputs); //, maxc);

                    //Impact Sound
                    var pitch = totalPitchMultiplier * impactSound.Pitch(speed);

                    int maxc = impactSound.audioSources.Length;
                    soundOutputs.Downshift(maxc, impactSound.minimumTypeWeight);

                    vol = Mathf.Min(vol, impactSound.maxVolume);

                    var c = Mathf.Min(maxc, soundOutputs.Count);
                    for (int i = 0; i < c; i++)
                    {
                        var output = soundOutputs[i];
                        var st = soundSet.surfaceTypeSounds[output.surfaceTypeID];
                        var voll = vol * output.weight * output.volumeMultiplier;
                        st.PlayOneShot(impactSound.audioSources[i], voll, pitch * output.pitchMultiplier);
                    }

                    if (onEnterSound != null)
                        onEnterSound(collision, soundOutputs);


                    //Impact Particles
                    if (doParticles)
                    {
                        float approximateCollisionDuration = 1 / Mathf.Max(0.00000001f, particles.selfHardness * soundOutputs.hardness);

                        particleOutputs.Downshift(MAX_PARTICLE_TYPE_COUNT, particles.minimumTypeWeight);

                        Calculate(collision, out int contactCount, out Vector3 center, out Vector3 normal, out float radius, out Vector3 vel0, out Vector3 vel1, out float mass0, out float mass1);
                        DoParticles(collision, particleOutputs, approximateCollisionDuration, center, normal, radius, vel0, vel1, mass0, mass1, absImpulse, absSpeed);

                        if (onEnterParticles != null)
                            onEnterParticles(collision, particleOutputs);
                    }
                }
            }
        }

        private void Update()
        {
            impactCooldownT -= Time.deltaTime;

            if(doVibrationSound)
            {
                vibrationSound.Update(totalVolumeMultiplier, totalPitchMultiplier);
            }

            if (doFrictionSound)
            {
                //Downshifts and reroutes
                if (!downShifted)
                {
                    downShifted = true;

                    //Re-sorts them
                    averageOutputs.SortDescending();

                    //Downshifts
                    var maxCount = frictionSound.sources.Length;
                    averageOutputs.Downshift(maxCount, frictionSound.minimumTypeWeight);

                    //Clears Givens
                    for (int i = 0; i < maxCount; i++)
                        frictionSound.sources[i].given = false;
                    givenSources.Clear();

                    //Sees if any of them are aligned
                    for (int outputID = 0; outputID < averageOutputs.Count; outputID++)
                    {
                        var c = Mathf.Min(maxCount, averageOutputs.Count); //?

                        var output = averageOutputs[outputID];
                        var clip = soundSet.surfaceTypeSounds[output.surfaceTypeID].frictionSound;

                        //Finds and assigns sources that match the clip already
                        int givenSource = -1;
                        for (int sourceID = 0; sourceID < frictionSound.sources.Length; sourceID++)
                        {
                            var source = frictionSound.sources[sourceID];

                            if (source.clip == clip || source.Silent || source.clip == null || source.clip.clip == null)
                            {
                                source.ChangeClip(clip, this);

                                givenSource = sourceID;
                                source.given = true;
                                break;
                            }
                        }
                        givenSources.Add(givenSource);
                    }

                    //Changes Clips
                    for (int outputID = 0; outputID < averageOutputs.Count; outputID++)
                    {
                        var output = averageOutputs[outputID];

                        //If it wasn't given a source
                        if (givenSources[outputID] == -1)
                        {
                            var clip = soundSet.surfaceTypeSounds[output.surfaceTypeID].frictionSound;

                            for (int sourceID = 0; sourceID < frictionSound.sources.Length; sourceID++)
                            {
                                var source = frictionSound.sources[sourceID];

                                if (!source.given)
                                {
                                    source.given = true;

                                    source.ChangeClip(clip, this);
                                    givenSources[outputID] = sourceID;

                                    break;
                                }
                            }
                        }
                    }
                }


#if UNITY_EDITOR
                currentFrictionDebug = "";
#endif

                float speed = 0;
                if (forceSum > 0) //prevents a divide by zero
                    speed = weightedSpeed / forceSum;

                //Updates the sources which have been given
                for (int outputID = 0; outputID < averageOutputs.Count; outputID++)
                {
                    var output = averageOutputs[outputID];
                    var source = frictionSound.sources[givenSources[outputID]];

#if UNITY_EDITOR
                    var st = soundSet.surfaceTypeSounds[output.surfaceTypeID];
                    currentFrictionDebug = currentFrictionDebug + st.name + " V: " + output.weight + " P: " + output.pitchMultiplier + "\n";
#endif

                    var vm = totalVolumeMultiplier * output.volumeMultiplier;
                    var pm = totalPitchMultiplier * output.pitchMultiplier;
                    source.Update(frictionSound, vm, pm, forceSum * output.weight, speed);
                }

                //Updates the sources which haven't been given
                for (int i = 0; i < frictionSound.sources.Length; i++)
                {
                    var source = frictionSound.sources[i];
                    if (!source.given)
                    {
                        source.Update(frictionSound, totalVolumeMultiplier, totalPitchMultiplier, 0, 0);
                    }
                }
            }
        }
    }
}

/*
        public const bool CLAMP_FINAL_ONE_SHOT_VOLUME = true;
 *             
 *             //if (CLAMP_FINAL_ONE_SHOT_VOLUME)
                        //    voll = Mathf.Min(voll, 1);

            float impulse = 0;
            for (int i = 0; i < contactCount; i++)
            {
                var contact = contacts[i];
                var norm = contact.normal;
                impulse += impMag * Mathf.Lerp(1, frictionSound.frictionNormalForceMultiplier, Mathf.Abs(Vector3.Dot(normImp, norm))); //I'm not sure if this works

                speed += Vector3.ProjectOnPlane(CurrentRelativeVelocity(contact), norm).magnitude;
            }
            float invCount = 1f / contactCount;
            impulse *= invCount;
            speed *= speedMultiplier * invCount; // Vector3.ProjectOnPlane(CurrentRelativeVelocity(collision), contact.normal).magnitude; // collision.relativeVelocity.magnitude;


        private Vector3 CurrentRelativeVelocity(ContactPoint contact)
        {
            //return collision.relativeVelocity.magnitude;

            Vector3 Vel(Rigidbody rb, Vector3 pos)
            {
                if (rb == null)
                    return Vector3.zero;
                return rb.GetPointVelocity(pos);
            }

            //This version takes into account angular, I believe Unity's doesn't

            //TODO: make it use multiple contacts?

            var vel = Vel(contact.thisCollider.attachedRigidbody, contact.point);
            var ovel = Vel(contact.otherCollider.attachedRigidbody, contact.point);

            return (vel - ovel); //.magnitude;
        }

 *         //var norm = collision.GetContact(0).normal;

        //Debug.DrawRay(collision.GetContact(0).point, collision.impulse.normalized * 3);

        //Friction Sound
        //var force = Vector3.ProjectOnPlane(collision.impulse, norm).magnitude / Time.deltaTime; //Finds tangent force
        //var impulse = collision.impulse;
        //var force = (1 - Vector3.Dot(impulse.normalized, norm)) * impulse.magnitude / Time.deltaTime; //Finds tangent force
            //Debug.Log(collision.collider.gameObject.name + " " + collision.impulse.magnitude);
*/