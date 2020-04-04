/*
MIT License

Copyright (c) 2020 Steffen Vetne

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Make the speeds and forces be individual to each surface type, so that you can slide on multiple? but that's unfair to both STs of different collisions being the same

namespace PrecisionSurfaceEffects
{
    public partial class CollisionSounds : MonoBehaviour
    {
        //Constants
        private const float EXTRA_SEARCH_THICKNESS = 0.01f;
        private const int MAX_OUTPUT_MULT = 2; //because they will be blended, I can't be sure they have been sorted and culled properly yet



        //Fields
#if UNITY_EDITOR
        [Header("Debug")]
        [TextArea(1, 10)]
        //[ReadOnly]
        public string currentFrictionDebug;
#endif

        [Header("Quality")]
        [Space(30)]
        [Tooltip("Non-convex MeshCollider submeshes")]
        public bool findMeshColliderSubmesh = true;

        [Header("Scaling")]
        [Tooltip("To easily make the speed be local/relative")]
        public float speedMultiplier = 1;
        public float forceMultiplier = 1;
        public float totalVolumeMultiplier = 0.3f;
        public float totalPitchMultiplier = 1;

        [Header("Sounds")]
        public SurfaceSoundSet soundSet;

        [Header("Friction Sound")]
        [Space(15)]
        public FrictionSound frictionSound = new FrictionSound(); //[Tooltip("When the friction soundType changes, this can be used to play impactSound")]
        [Min(0)]
        public float frictionNormalForceMultiplier = 0.3f;

        [Header("Impact Sound")]
        [Space(15)]
        public Sound impactSound = new Sound();
        public float impactCooldown = 0.1f; //public FrictionTypeChangeImpact frictionTypeChangeImpact = new FrictionTypeChangeImpact();
        public float impulseChangeToImpact = 100;

        //Impact Sound
        private float impactCooldownT;

        //Friction Sound
        private readonly SurfaceOutputs averageOutputs = new SurfaceOutputs(); // List<CollisionSound> collisionSounds = new List<CollisionSound>();
        private readonly List<int> givenSources = new List<int>();
        private float weightedSpeed;
        private float forceSum;
        private bool downShifted;

        private float previousImpulse;



        //Methods
        private SurfaceOutputs GetSurfaceTypeOutputs(Collision c, int maxOutputs)
        {
            if (findMeshColliderSubmesh && c.collider is MeshCollider mc && !mc.convex)
            {
                var contact = c.GetContact(0);
                var pos = contact.point;
                var norm = contact.normal; //this better be normalized!

                float searchThickness = EXTRA_SEARCH_THICKNESS + Mathf.Abs(contact.separation);

                if (mc.Raycast(new Ray(pos + norm * searchThickness, -norm), out RaycastHit rh, Mathf.Infinity)) //searchThickness * 2
                {
#if UNITY_EDITOR
                    float debugSize = 3;
                    Debug.DrawLine(pos + norm * debugSize, pos - norm * debugSize, Color.white, 0);
#endif

                    SurfaceData.outputs.Clear();
                    soundSet.data.AddSurfaceTypes(SurfaceData.outputs, c.collider, pos, maxOutputs, triangleIndex: rh.triangleIndex);
                    return SurfaceData.outputs;
                }
            }

            return soundSet.data.GetCollisionSurfaceTypes(c, maxOutputs);
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

#if UNITY_EDITOR
        private void OnValidate()
        {
            if(!Application.isPlaying)
                currentFrictionDebug = "(This only works in playmode)";

            impactSound.Validate(false);
            frictionSound.Validate(true);
        }
#endif

        private void FixedUpdate()
        {
            averageOutputs.Clear(); // collisionSounds.Clear();
            downShifted = false;
            weightedSpeed = 0;
            forceSum = 0;
        }

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

        private void OnCollisionEnter(Collision collision)
        {
            //Impact Sound
            if (impactCooldownT <= 0)
            {
                impactCooldownT = impactCooldown;

                var speed = speedMultiplier * collision.relativeVelocity.magnitude; //Can't consistently use CurrentRelativeVelocity(collision);, probably maybe because it's too late to get that speed (already resolved)
                var force = forceMultiplier * collision.impulse.magnitude;//Here "force" is actually an impulse
                var vol = totalVolumeMultiplier * impactSound.Volume(force) * impactSound.SpeedFader(speed);
                var pitch = totalPitchMultiplier * impactSound.Pitch(speed);

                int maxc = impactSound.audioSources.Length;
                var outputs = GetSurfaceTypeOutputs(collision, maxc);
                outputs.Downshift(maxc, impactSound.minimumTypeWeight);

                var c = Mathf.Min(maxc, outputs.Count);
                for (int i = 0; i < c; i++)
                {
                    var output = outputs[i];
                    var st = soundSet.surfaceTypeSounds[output.surfaceTypeID];
                    st.PlayOneShot(impactSound.audioSources[i], vol * output.weight * output.volume, pitch * output.pitch);
                }
            }
        }
        private void OnCollisionStay(Collision collision)
        {
            var imp = collision.impulse;
            var impMag = imp.magnitude;
            var normImp = imp.normalized;

            float speed = 0;
            float impulse = 0;
            int contactCount = collision.contactCount;
            for (int i = 0; i < contactCount; i++)
            {
                var contact = collision.GetContact(0);
                var norm = contact.normal;
                impulse += impMag * Mathf.Lerp(1, frictionNormalForceMultiplier, Mathf.Abs(Vector3.Dot(normImp, norm)));

                speed += CurrentRelativeVelocity(contact).magnitude;
            }
            float invCount = 1 / contactCount;
            impulse *= forceMultiplier * invCount;
            speed *= speedMultiplier * invCount; // Vector3.ProjectOnPlane(CurrentRelativeVelocity(collision), contact.normal).magnitude; // collision.relativeVelocity.magnitude;

            var force = impulse / Time.deltaTime; //force = Mathf.Max(0, Mathf.Min(frictionSound.maxForce, force) - frictionSound.minForce);
            force *= frictionSound.SpeedFader(speed); //So that it is found the maximum with this in mind

            if (impulse - previousImpulse >= impulseChangeToImpact)
            {
                Debug.Log(impulse - previousImpulse);
                OnCollisionEnter(collision);
            }
            previousImpulse = impulse;

            if (force > 0)
            {
                var outputs = GetSurfaceTypeOutputs(collision, frictionSound.sources.Length * MAX_OUTPUT_MULT);

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
                        var sumOutput = averageOutputs[ii];
                        if (sumOutput.surfaceTypeID == output.surfaceTypeID)
                        {
                            sumOutput.weight = invInfluence * sumOutput.weight + influence * output.weight;
                            sumOutput.volume = invInfluence * sumOutput.volume + influence * output.volume;
                            sumOutput.pitch = invInfluence * sumOutput.pitch + influence * output.pitch;
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
                                surfaceTypeID = output.surfaceTypeID,
                                weight = output.weight * influence,
                                volume = output.volume * influence,
                                pitch = output.pitch * influence,
                            }
                        );
                    }
                }
            }
        }

        private void Update()
        {
            impactCooldownT -= Time.deltaTime;


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
                    var clip = soundSet.surfaceTypeSounds[output.surfaceTypeID].loopSound;

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
                        var clip = soundSet.surfaceTypeSounds[output.surfaceTypeID].loopSound;
          
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
                currentFrictionDebug = currentFrictionDebug + st.name + " V: " + output.weight + " P: " + output.pitch + "\n";
#endif

                var vm = totalVolumeMultiplier * output.volume;
                var pm = totalPitchMultiplier * output.pitch;
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

/*
 *         //var norm = collision.GetContact(0).normal;

        //Debug.DrawRay(collision.GetContact(0).point, collision.impulse.normalized * 3);

        //Friction Sound
        //var force = Vector3.ProjectOnPlane(collision.impulse, norm).magnitude / Time.deltaTime; //Finds tangent force
        //var impulse = collision.impulse;
        //var force = (1 - Vector3.Dot(impulse.normalized, norm)) * impulse.magnitude / Time.deltaTime; //Finds tangent force
*/