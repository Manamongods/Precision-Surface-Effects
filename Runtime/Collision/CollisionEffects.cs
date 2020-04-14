/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Make sure that ContactPoint always works the same

namespace PrecisionSurfaceEffects
{
    public sealed partial class CollisionEffects : CollisionEffectsMaker, IOnOnCollisionStay
    {
        //Constants
        public static float EXTRA_SEARCH_THICKNESS = 0.01f;
        public static int MAX_PARTICLE_TYPE_COUNT = 10;
        public static float IMPACT_DURATION_CONSTANT = .1f;
        public static bool CLAMP_FINAL_ONE_SHOT_VOLUME = false; //true;
        public static float AUDIBLE_THRESHOLD = 0.00001f; //this is important especially because SmoothDamp never reaches the target
        public static float TOUCHING_SEPARATION_THRESHOLD = 0.01f;



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
        [Min(0.0001f)]
        public float impactCooldown = 0.1f;
        [Space(5)]
        public bool doSpeculativeImpacts;
        public float separationThresholdMultiplier = 1;
        public float minimumAngleDifference = 30;
        //public float maximumContactRelocationRate = 100; //This probably only works for "Persistent Contact Manifold" Contacts Generation
        //public float maximumContactRotationRate = 180;
        [Space(5)]
        public bool doImpactByForceChange = true;
        public float forceChangeToImpact = 10;

        [SeperatorLine]
        [Header("Sounds")]
        [Space(30)]
        public SurfaceSoundSet soundSet;

        [Header("Scaling")]
        [Space(5)]
        [Tooltip("To easily make the sound speed be local/relative")] //These 4 apply only to sounds, not particles
        public float soundSpeedMultiplier = 1;
        public float soundForceMultiplier = 1;
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

        [SerializeField][HideInInspector]
        private Rigidbody rb;
        [SerializeField][HideInInspector]
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

        private readonly List<Contact> contacts = new List<Contact>();

        private readonly List<int> ids = new List<int>();
        private readonly List<int> bestIDs = new List<int>();

        private static readonly ContactPoint[] dumbCPs = new ContactPoint[64];
        private static readonly List<ContactPoint> contactPoints = new List<ContactPoint>();
        private static readonly List<ContactPoint> speculativePoints = new List<ContactPoint>();

        private static readonly Queue<Contact> availableContacts = new Queue<Contact>();



        //Properties
        private bool NeedOnCollisionStay
        {
            get
            {
                bool wanted = particlesType == ParticlesType.ImpactAndFriction || doFrictionSound || doSpeculativeImpacts;
                bool canReceiveCallbacks = collider != null && (collider.attachedRigidbody == null || rb != null); //This is (still) correct right?
                return wanted && canReceiveCallbacks;
            }
        }



        //Methods
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

        public static float GetApproximateImpactDuration(float hardness0, float hardness1)
        {
            return IMPACT_DURATION_CONSTANT / Mathf.Max(0.00000001f, hardness0 * hardness1);
        }

        public static Vector3 GetRollingVelocity(Vector3 vel0, Vector3 vel1, Vector3 cvel0, Vector3 cvel1)
        {
                //find the movement of the contact 

            var roll0 = (cvel0 - vel1);
            var roll1 = (cvel1 - vel0);
            return roll1 - roll0;
        }

        public static SurfaceOutputs GetSmartCollisionSurfaceTypes(ContactPoint c, bool findMeshColliderSubmesh, SurfaceData data)
        {
            if (findMeshColliderSubmesh && RaycastTestSubmesh(c, out RaycastHit rh, out Vector3 pos))
            {
                SurfaceData.outputs.Clear();
                data.AddSurfaceTypes(c.otherCollider, pos, triangleIndex: rh.triangleIndex);
                return SurfaceData.outputs;
            }

            return data.GetContactSurfaceTypes(c, shareList: true);
        }
        private static bool RaycastTestSubmesh(ContactPoint contact, out RaycastHit rh, out Vector3 pos)
        {
            if (contact.otherCollider is MeshCollider mc && !mc.convex)
            {
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
        private void GetSurfaceTypeOutputs(ContactPoint c, bool doSound, bool doParticle, out SurfaceOutputs soundOutputs, out SurfaceOutputs particleOutputs)
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
                        data.AddSurfaceTypes(c.otherCollider, pos, triangleIndex: rh.triangleIndex);
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
                    soundSet.data.GetContactSurfaceTypes(c, shareList: true);
                    soundOutputs = GetFlipFlopOutputs();
                }

                if (doParticle)
                {
                    particleSet.data.GetContactSurfaceTypes(c, shareList: true);
                    particleOutputs = GetFlipFlopOutputs();
                }
            }
        }

        private bool Stop(Collider collider, bool friction)
        {
            //This prevents multiple sounds for one collision

            var rb = collider.attachedRigidbody;
            Transform target;
            if (rb != null)
                target = rb.transform;
            else
                target = collider.transform;

            var otherCSM = target.GetComponent<CollisionEffectsMaker>();
            if (otherCSM != null)
            {
                if (otherCSM.priority == priority)
                {
                    //This complicated stuff gives randomish (but they both get the same value) results as to whether this or the other should be the one to produce effects

                    bool bigger = (otherCSM.gameObject.GetInstanceID() > gameObject.GetInstanceID());
                    if (!friction)
                    {
                        return bigger ^ FrameBool();
                    }
                    else
                    {
                        bool flipBool;
                        if (bigger)
                            flipBool = stayFrameBool;
                        else
                            flipBool = otherCSM.stayFrameBool;

                        return bigger ^ flipBool;
                    }
                }

                return priority < otherCSM.priority;
            }
            return false;
        }

        private void DoFrictionSound(SurfaceOutputs outputs, float impulse, float speed, float force)
        {
            if (force > 0)
            {
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

                            void LerpPM(ref ParticleMultipliers from, ParticleMultipliers to)
                            {
                                from.countMultiplier = invInfluence * from.countMultiplier + influence * to.countMultiplier;
                                from.sizeMultiplier = invInfluence * from.sizeMultiplier + influence * to.sizeMultiplier;
                            }

                            Lerp(ref o.weight, output.weight);
                            Lerp(ref o.volumeMultiplier, output.volumeMultiplier);
                            Lerp(ref o.pitchMultiplier, output.pitchMultiplier);
                            LerpPM(ref o.selfParticleMultipliers, output.selfParticleMultipliers);
                            LerpPM(ref o.otherParticleMultipliers, output.otherParticleMultipliers);
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
                                selfParticleMultipliers = output.selfParticleMultipliers,
                                otherParticleMultipliers = output.otherParticleMultipliers,
                                color = output.color,
                            }
                        );
                    }
                }
            }
        }

        private void Calculate(List<ContactPoint> contactPoints, out Vector3 center, out Vector3 normal, out float radius, out Vector3 vel0, out Vector3 vel1, out Vector3 cvel0, out Vector3 cvel1, out float mass0, out float mass1)
        {
            radius = 0;

            var c0 = contactPoints[0];

            int contactCount = contactPoints.Count;
            if (contactCount == 1)
            {
                var contact = c0;
                center = contact.point;
                normal = contact.normal;
            }
            else
            {
                normal = new Vector3();
                center = new Vector3();

                for (int i = 0; i < contactCount; i++)
                {
                    var contact = contactPoints[i];
                    normal += contact.normal;
                    center += contact.point;
                }

                normal.Normalize();
                float invCount = 1f / contactCount;
                center *= invCount;

                for (int i = 0; i < contactCount; i++)
                {
                    var contact = contactPoints[i];
                    radius += (contact.point - center).magnitude; //this doesn't care if it is perpendicular to normal, but should it?
                }

                radius *= invCount;
            }

            vel0 = Utility.GetVelocityMass(c0.thisCollider.attachedRigidbody, center, out cvel0, out mass0);
            vel1 = Utility.GetVelocityMass(c0.otherCollider.attachedRigidbody, center, out cvel1, out mass1); //collision.rigidbody


            //#if UNITY_EDITOR
            //            Debug.DrawRay(center, normal, Color.red);
            //            Debug.DrawRay(center, vel0 - vel1, Color.green);
            //#endif
        }


        private Contact GetContact()
        {
            if(availableContacts.Count > 0)
            {
                var c = availableContacts.Dequeue();
                c.contactPoints0.Clear();
                c.contactPoints1.Clear();
                c.locals0.Clear();
                c.locals1.Clear();
                c.impulse = new Vector3(); //c.linearVelocity = Vector3.zero;
                c.used = false;
                return c;
            }
            else
            {
                return new Contact();
            }
        }


        private void DoParticles(SurfaceOutputs particleOutputs, float dt, Vector3 center, Vector3 normal, float radius, Vector3 vel0, Vector3 vel1, Vector3 cvel0, Vector3 cvel1, float mass0, float mass1, float impulse, float speed, bool isFriction, float rollingSpeed = 0)
        {
            if (particleOutputs.Count != 0) //particleSet != null && 
            {
                var rot = Quaternion.FromToRotation(Vector3.forward, normal); //Vector3.up

                particleOutputs.Downshift(MAX_PARTICLE_TYPE_COUNT, particles.minimumTypeWeight);

                for (int i = 0; i < particleOutputs.Count; i++)
                {
                    var o = particleOutputs[i];

                    var sps = particleSet.GetSurfaceParticles(o);

                    if (sps != null)
                    {
                        for (int ii = 0; ii < sps.Length; ii++)
                        {
                            var sp = sps[ii];
                            var spp = sp.particles;

                            var fadingSpeed = isFriction ? spp.GetAmountedSpeed(rollingSpeed, speed) : spp.impactSpeedMultiplier * speed;
                            float speedFader = particles.SpeedFader(fadingSpeed);

                            var selfMults = o.selfParticleMultipliers * particles.selfMultipliers * sp.selfMultipliers;
                            var otherMults = o.otherParticleMultipliers * particles.otherMultipliers * sp.otherMultipliers;

                            spp.GetInstance().PlayParticles
                            (
                                sp.originType,
                                particles.selfColor, o.color,
                                selfMults, otherMults,
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
        }

        public void OnOnCollisionStay(Collision collision)
        {
            if (!isActiveAndEnabled)
                return;


            //Finds Impulse
            Vector3 impulseNormal = collision.impulse;
            float impulse = impulseNormal.magnitude;
            float absImpulse = impulse;
            if (impulse != 0)
                impulseNormal /= impulse;
            impulse *= soundForceMultiplier;


            //Gets Contact Points
            int contactCount = collision.GetContacts(dumbCPs);
            contactPoints.Clear();
            for (int i = 0; i < contactCount; i++) //Debug.Log(contactPoints.Count + " " +  collision.contactCount);
                contactPoints.Add(dumbCPs[i]);


            //Caches
            var c0 = contactPoints[0];
            var thisCollider = c0.thisCollider;
            var collider = collision.collider;


            bool stop = Stop(collision.collider, true);
            if (stop && !doVibrationSound)
                return;


            #region Finds Contact
            Contact contact = null;
            for (int i = 0; i < contacts.Count; i++)
            {
                var c = contacts[i];
                if (c.otherCollider == collider && c.thisCollider == thisCollider)
                {
                    contact = c;
                    break;
                }
            }
            #endregion

            if (contact != null) //!stop && 
            {
                contact.used = true;


                //Speculative Impacts
                if (doSpeculativeImpacts)
                {
                    #region Finds Best IDs
                    bestIDs.Clear();

                    contact.Flip();
                    var selfTransform = c0.thisCollider.transform;
                    var otherTransform = c0.otherCollider.transform;
                    contact.Update(selfTransform, contactPoints); //otherTransform, 

                    float bestDistance = Mathf.Infinity;


                    int currentCount = contact.contactPoints0.Count;
                    int previousCount = contact.contactPoints1.Count;

                    void Find(float distanceSum, int empties, int id) //int usedCount, 
                    {
                        if (id == currentCount)
                        {
                            if (distanceSum < bestDistance)
                            {
                                bestDistance = distanceSum;
                                bestIDs.Clear();
                                bestIDs.AddRange(ids);
                            }

                            return;
                        }

                        Vector3 locPos = contact.locals0[id]; //.position;

                        if (empties > 0)
                        {
                            ids.Add(-1);
                            Find(distanceSum, empties - 1, id + 1);
                            ids.RemoveAt(id);
                        }

                        for (int i = 0; i < previousCount; i++)
                        {
                            bool available = true;

                            for (int ii = 0; ii < ids.Count; ii++)
                            {
                                if (ids[ii] == i)
                                {
                                    available = false;
                                    break;
                                }
                            }

                            if (available)
                            {
                                Vector3 prevLocPos = contact.locals1[i]; //.position;

                                float newDistance = (locPos - prevLocPos).sqrMagnitude; //Make this just magnitude?

                                ids.Add(i);
                                Find(distanceSum + newDistance, empties, id + 1);
                                ids.RemoveAt(id);
                            }
                        }
                    }

                    ids.Clear();
                    Find(0, currentCount - previousCount, 0);
                    #endregion

                    #region Does Speculative Contacts
                    var col = collision.collider;

                    bool Touching(float separation)
                    {
                        return separation <= TOUCHING_SEPARATION_THRESHOLD * separationThresholdMultiplier;
                    }

                    speculativePoints.Clear();
                    for (int i = 0; i < bestIDs.Count; i++)
                    {
                        var cp = contactPoints[i];

#if UNITY_EDITOR
                        if (bestIDs[i] != -1)
                            Debug.DrawLine(cp.point, contact.contactPoints1[bestIDs[i]].point, Color.magenta); //Draws to the guessed previous location
#endif

                        if (Touching(cp.separation))
                        {
#if UNITY_EDITOR
                            Debug.DrawRay(cp.point, Vector3.right, Color.green); //Draws a green sideways ray if the point is currently touching
#endif

                            var id = bestIDs[i];
                            if (id == -1)
                            {
                                //Frequently it seems PhysX has e.g. 2 contacts in one location, and then suddenly creates a new one in the same location.

                                float minimumAngle = Mathf.Infinity;
                                for (int ii = 0; ii < contactPoints.Count; ii++)
                                {
                                    if(i != ii)
                                        minimumAngle = Mathf.Min(minimumAngle, Vector3.Angle(contactPoints[i].normal, contactPoints[ii].normal));
                                }

                                if(minimumAngle > minimumAngleDifference)
                                    speculativePoints.Add(cp);
                            }
                            else
                            {
                                var prevCP = contact.contactPoints1[id];
                                bool previouslyTouching = Touching(prevCP.separation); //If not previously touching (I'm basing this on the idea that contacts are created before the impact, which seems to be quite an accurate assumption most of the time)

                                //var local = contact.locals0[i];
                                //var prevLocal = contact.locals1[id];

                                //var worldNormal = cp.normal;
                                //float change = Vector3.Angle(worldNormal, otherTransform.TransformDirection(prevLocal.otherNormal));
                                //change = Mathf.Min(change, Vector3.Angle(worldNormal, selfTransform.TransformDirection(prevLocal.normal)));
                                //change = Mathf.Min(change, Vector3.Angle(local.otherNormal, prevLocal.otherNormal));
                                //change = Mathf.Min(change, Vector3.Angle(local.normal, prevLocal.normal));
                                //Debug.Log(change / Time.deltaTime);

                                //bool properRotation = change / Time.deltaTime < maximumContactRotationRate;
                                //bool properTranslation = Vector3.Distance(local.position, prevLocal.position) / Time.deltaTime < maximumContactRelocationRate;

                                if (!previouslyTouching) // || !properTranslation || !properRotation)
                                {
                                    speculativePoints.Add(cp);
                                }
                            }
                        }
                    }

                    if (speculativePoints.Count > 0)
                    {
#if UNITY_EDITOR
                        for (int i = 0; i < speculativePoints.Count; i++)
                            Debug.DrawRay(speculativePoints[i].point, Vector3.up * 100); //Draws a white ray upward if a new impact is created there
#endif

                        OnOnCollisionEnter(stop, collider, speculativePoints, collision.impulse - contact.impulse, collision.relativeVelocity); //linearVelocity //TODO: find point velocity //Unfortunately the velocity is NOT point velocity
                    }
                    #endregion
                }


                //Impact by Force Change
                if (doImpactByForceChange)
                {
                    var change = collision.impulse - contact.impulse;

                    if (change.magnitude / Time.deltaTime >= forceChangeToImpact) //(imp.magnitude - contact.impulse)
                    {
                        OnOnCollisionEnter(stop, collider, contactPoints, change, collision.relativeVelocity); //linearVelocity //OnCollisionEnter(collision);
                    }
                }


                contact.impulse = collision.impulse;
                //contact.relativeVelocity = collision.relativeVelocity; // RememberVelocities(c0);
            }


            var doParticles = particlesType == ParticlesType.ImpactAndFriction;
            SurfaceOutputs soundOutputs = null, particleOutputs = null;
            if(!stop)
                GetSurfaceTypeOutputs(contactPoints[0], doFrictionSound, doParticles, out soundOutputs, out particleOutputs);


            //Calculation
            Calculate(contactPoints, out Vector3 center, out Vector3 normal, out float radius, out Vector3 vel0, out Vector3 vel1, out Vector3 cvel0, out Vector3 cvel1, out float mass0, out float mass1);

            float perpendicularSpeed = Vector3.ProjectOnPlane(vel1 - vel0, normal).magnitude;
            float frictionImpulser = Mathf.Lerp(1, frictionSound.frictionNormalForceMultiplier, Mathf.Abs(Vector3.Dot(impulseNormal, normal))); //I'm not sure if this works //Debug.Log(perpendicularSpeed + "   " + (vel1 - vel0).magnitude + "    " + Vector3.Dot((vel0 - vel1).normalized, normal.normalized) + "    " + (vel0 - vel1));
            //lerp unclapmed?

            var rollingVelocity = GetRollingVelocity(vel0, vel1, cvel0, cvel1);


            float rollingSpeed = Vector3.ProjectOnPlane(rollingVelocity, normal).magnitude; //centerSpeed //The reason is because perpendicularSpeed is for slide sounds, and centerSpeed is for roll sounds. But they are both considered friction sounds here
            float frictionSpeed = perpendicularSpeed * frictionSound.slidingAmount + rollingSpeed * frictionSound.rollingAmount; //Mathf.Max();
            frictionSpeed *= soundSpeedMultiplier;

            var frictionForce = impulse / Time.deltaTime; //force = Mathf.Max(0, Mathf.Min(frictionSound.maxForce, force) - frictionSound.minForce);
            frictionForce *= frictionSound.SpeedFader(frictionSpeed); //So that it is found the maximum with this in mind


            if (doVibrationSound)
            {
                vibrationSound.Add(frictionForce * vibrationSound.frictionForceMultiplier, frictionSpeed * vibrationSound.frictionSpeedMultiplier);

                if (stop)
                    return;
            }


            //Friction Sounds
            if (doFrictionSound)
            {
                DoFrictionSound(soundOutputs, frictionImpulser * impulse, frictionSpeed, frictionForce);
            }


            //Particles
            if (doParticles)
                DoParticles(particleOutputs, Time.deltaTime, center, normal, radius, vel0, vel1, cvel0, cvel1, mass0, mass1, frictionImpulser * absImpulse, perpendicularSpeed, true, rollingVelocity.magnitude);
        }

        private void OnOnCollisionEnter(bool stop, Collider collider, List<ContactPoint> contactPoints, Vector3 impulseVector, Vector3 relativeVelocity)
        {
            stop = stop || impactCooldownT > 0; //Impact cooldown is actually quite arbitrarily influenced by whether it is Stopped or not

            if (!stop || doVibrationSound)
            {
                var absSpeed = relativeVelocity.magnitude;
                var speed = soundSpeedMultiplier * absSpeed;
                float speedFade = impactSound.SpeedFader(speed);

                var absImpulse = impulseVector.magnitude;
                var impulse = soundForceMultiplier * absImpulse;
                var vol = totalVolumeMultiplier * impactSound.Volume(impulse) * speedFade; //force //Here "force" is actually an impulse

                if (vol > 0.000000000001f)
                {
                    if (doVibrationSound)
                    {
                        vibrationSound.Add(impulse * vibrationSound.impactForceMultiplier * speedFade, speed * vibrationSound.impactSpeedMultiplier); //Remove speedFade?
                    }

                    if (stop)
                        return;

                    impactCooldownT = impactCooldown;

                    bool doParticles = particlesType != ParticlesType.None;
                    GetSurfaceTypeOutputs(contactPoints[0], true, doParticles, out SurfaceOutputs soundOutputs, out SurfaceOutputs particleOutputs);

                    #region Impact Sound
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
                        if (CLAMP_FINAL_ONE_SHOT_VOLUME)
                            voll = Mathf.Min(voll, 1);
                        st.PlayOneShot(impactSound.audioSources[i], voll, pitch * output.pitchMultiplier);
                    }
                    #endregion

                    #region Impact Particles
                    if (doParticles)
                    {
                        float approximateCollisionDuration = GetApproximateImpactDuration(particles.selfHardness, soundOutputs.hardness);

                        particleOutputs.Downshift(MAX_PARTICLE_TYPE_COUNT, particles.minimumTypeWeight);

                        Calculate
                        (
                            contactPoints,
                            out Vector3 center, out Vector3 normal, out float radius, 
                            out Vector3 vel0, out Vector3 vel1, out Vector3 cvel0, out Vector3 cvel1, 
                            out float mass0, out float mass1
                        );

                        DoParticles
                        (
                            particleOutputs, approximateCollisionDuration, 
                            center, normal, radius, 
                            vel0, vel1, cvel0, cvel1, 
                            mass0, mass1, 
                            absImpulse, absSpeed, 
                            isFriction: false
                        );
                    }
                    #endregion
                }
            }
        }



        //Datatypes
        public enum ParticlesType { None, ImpactOnly, ImpactAndFriction }

        public delegate void OnSurfaceCallback(Collision collision, SurfaceOutputs outputs);

        private class Contact
        {
            //Fields
            public bool used;

            public Collider thisCollider;
            public Collider otherCollider;

            public Vector3 impulse;

            private const int DEF_C = 8;
            public List<ContactPoint> contactPoints0 = new List<ContactPoint>(DEF_C);
            public List<ContactPoint> contactPoints1 = new List<ContactPoint>(DEF_C);
            public List<Vector3> locals0 = new List<Vector3>(DEF_C); //Local
            public List<Vector3> locals1 = new List<Vector3>(DEF_C);
            
            //public struct Local
            //{
            //    public Vector3 position; //, normal;
            //    //public Vector3 otherPosition, otherNormal;
            //}

            //public Vector3 relativeVelocity; //previous

            //public Vector3 linearVelocity;
            //public Vector3 angularVelocity;


            //Methods
            //public void RememberVelocities(ContactPoint c)
            //{
            //    var rb = c.thisCollider.attachedRigidbody;
            //    if (rb == null)
            //    {
            //        angularVelocity = linearVelocity = Vector3.zero;
            //    }
            //    else
            //    {
            //        angularVelocity = rb.angularVelocity;
            //        linearVelocity = rb.velocity;
            //    }
            //}

            public void Flip()
            {
                var temp = contactPoints0;
                contactPoints0 = contactPoints1;
                contactPoints1 = temp;

                var temp2 = locals0;
                locals0 = locals1;
                locals1 = temp2;
            }

            public void Update(Transform t, List<ContactPoint> ps) //Transform other, 
            {
                contactPoints0.Clear();
                locals0.Clear();

                contactPoints0.AddRange(ps);
                for (int i = 0; i < ps.Count; i++)
                {
                    var p = ps[i];

                    locals0.Add
                    (
                        t.InverseTransformPoint(p.point)

                        //new Local()
                        //{
                        //    position = t.InverseTransformPoint(p.point),
                            //normal = t.InverseTransformDirection(p.normal),

                            //otherPosition = other.InverseTransformPoint(p.point),
                            //otherNormal = other.InverseTransformDirection(p.normal)
                        //}
                    );
                }
            }
        }



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
            impactCooldownT = -1;

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
            averageOutputs.Clear();
            downShifted = false;
            weightedSpeed = 0;
            forceSum = 0;

            //Removes unused Contacts - might have a problem if a collision is exited in one frame and comes back the next?
            for (int i = 0; i < contacts.Count; i++)
            {
                var c = contacts[i];
                if (!c.used)
                {
                    availableContacts.Enqueue(c);
                    contacts.RemoveAt(i);
                    i--;
                }
                else
                    c.used = false;
            }
        }

        internal void OnCollisionEnter(Collision collision)
        {
            if (!isActiveAndEnabled)
                return;

            stayFrameBool = FrameBool();

            int contactCount = collision.GetContacts(dumbCPs);
            contactPoints.Clear();
            for (int i = 0; i < contactCount; i++) //Debug.Log(contactPoints.Count + " " +  collision.contactCount);
                contactPoints.Add(dumbCPs[i]);

            var c0 = contactPoints[0];

            var contact = GetContact();
            contact.thisCollider = c0.thisCollider;
            contact.otherCollider = c0.otherCollider;
            contact.Update(c0.thisCollider.transform, contactPoints); //? //c0.otherCollider.transform, 
            contact.used = true;
            contact.impulse = collision.impulse;
            //contact.relativeVelocity = collision.relativeVelocity; // RememberVelocities(c0);
            contacts.Add(contact);

            bool stop = Stop(c0.otherCollider, false);
            OnOnCollisionEnter(stop, c0.otherCollider, contactPoints, collision.impulse, collision.relativeVelocity); //Has to send collision.relativeVelocity because the speed is probably already resolved
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
                //Can't consistently use CurrentRelativeVelocity(collision);, probably maybe because it's too late to get that speed (already resolved)
 * 
 *        
                    if (onEnterSound != null)
                        onEnterSound(collision, soundOutputs);

                        if (onEnterParticles != null)
                            onEnterParticles(collision, particleOutputs);
        public OnSurfaceCallback onEnterParticles; //should I remove these?
        public OnSurfaceCallback onEnterSound;
     
 *             

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
