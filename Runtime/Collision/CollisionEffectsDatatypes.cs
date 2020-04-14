/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    public partial class CollisionEffects
    {
        [System.Serializable]
        public class Particles
        {
            [Min(0)]
            public float minimumTypeWeight = 0.1f;
            [Min(0)]
            public float selfHardness = 1;
            public Color selfColor = Color.white;
            public ParticleMultipliers selfMultipliers = ParticleMultipliers.Default();
            public ParticleMultipliers otherMultipliers = ParticleMultipliers.Default();
            [Min(0)]
            public float minimumParticleShapeRadius = 0;

            public Vector2 faderBySpeedRange = new Vector2(0.05f, 0.25f);

            public float SpeedFader(float speed)
            {
                return Mathf.Clamp01(Mathf.InverseLerp(faderBySpeedRange.x, faderBySpeedRange.y, speed));
            }
        }

        [System.Serializable]
        public class VibrationSound : LoopSource
        {
            //Fields
            [Min(0)]
            [Header("Volume")]
            public float volumeByForce = 0.1f;
            [Header("Pitch")]
            public float basePitch = 0.5f;
            public float pitchBySpeed = 0.035f;
            [Min(0)]
            [Space(10)]
            public float smoothTime = 0.05f;
            [Min(0)]
            public float maxForce = 10000;
            [Min(0)]
            public float maxVolume = 0.5f;
            [Header("Decay")]
            [Min(0)]
            public float exponentialDecay = 1;
            [Min(0)]
            public float linearDecay = 0.01f;

            [Header("Contribution")]
            [Min(0)]
            public float impactForceMultiplier = 1;
            public float impactSpeedMultiplier = -1;
            [Space(5)]
            [Min(0)]
            public float frictionForceMultiplier = 1;
            [Min(0)]
            public float frictionSpeedMultiplier = 1;

            internal float impulse; //albeit this is more of an impulse sum
            internal float weightedSpeed;

            private bool start;


            //Methods
            public void Add(float impulse, float speed)
            {
                if (this.impulse == 0)
                    start = true;

                this.impulse += impulse;
                weightedSpeed += impulse * speed;
            }

            public void Update(float totalVolumeMultiplier, float totalPitchMultiplier)
            {
                float prevImpulse = impulse;

                float multer = 1 / (1 + Time.deltaTime * exponentialDecay); //?????????
                impulse *= multer;
                impulse = Mathf.Max(0, impulse - Time.deltaTime * linearDecay);
                impulse = Mathf.Min(impulse, maxForce);

                float speedMulter = 0;
                if (prevImpulse != 0)
                    speedMulter = impulse / prevImpulse;
                weightedSpeed *= speedMulter;


                currentVolume = Mathf.SmoothDamp(currentVolume, volumeByForce * impulse, ref volumeVelocity, smoothTime);
                audioSource.volume = Mathf.Min(totalVolumeMultiplier * currentVolume, maxVolume);

                if (impulse > 0)
                {
                    float speed = weightedSpeed / impulse;
                    float targetPitch = basePitch + speed * pitchBySpeed;

                    if (start)
                    {
                        start = false;
                        currentPitch = targetPitch;
                        pitchVelocity = 0;
                    }

                    currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, smoothTime);
                    audioSource.pitch = totalPitchMultiplier * currentPitch;
                }

                EnsurePlayingOnlyIfAudible();
            }
        }

        [System.Serializable]
        public class Sound
        {
            //Fields
            [Tooltip("The more, the more surface types can be heard at once")]
            public AudioSource[] audioSources;
            [Min(0)]
            public float minimumTypeWeight = 0.1f;

            [Header("Volume")]
            [Min(0)]
            public float volumeByForce = 0.1f; //public float baseVolume = 0;
            public Vector2 volumeFaderBySpeedRange = new Vector2(0.01f, 0.1f);
            [Min(0)]
            public float maxVolume = 0.5f;

            [Header("Pitch")]
            public float basePitch = 0.5f;
            public float pitchBySpeed = 0.035f;


            //Methods
            public float SpeedFader(float speed)
            {
                return Mathf.Clamp01(Mathf.InverseLerp(volumeFaderBySpeedRange.x, volumeFaderBySpeedRange.y, speed));
            }

            public float Volume(float force)
            {
                return volumeByForce * force; //baseVolume + 
            }

            public float Pitch(float speed)
            {
                return basePitch + pitchBySpeed * speed;
            }

#if UNITY_EDITOR
            public void Validate(bool loop)
            {
                void Prepare(AudioSource source)
                {
                    source.loop = loop;
                    source.playOnAwake = false;
                }

                volumeFaderBySpeedRange.x = Mathf.Max(0, volumeFaderBySpeedRange.x);
                volumeFaderBySpeedRange.y = Mathf.Max(volumeFaderBySpeedRange.x, volumeFaderBySpeedRange.y);

                for (int i = 0; i < audioSources.Length; i++)
                    Prepare(audioSources[i]);
            }
#endif
        }

        public class LoopSource
        {
            //Fields
            public AudioSource audioSource;

            protected float currentVolume;
            protected float currentPitch;
            protected float volumeVelocity;
            protected float pitchVelocity;



            //Methods
            protected static bool Audible(float vol)
            {
                return vol > AUDIBLE_THRESHOLD;
            }

            protected void EnsurePlayingOnlyIfAudible()
            {
                bool audible = Audible(currentVolume);
                if (audible && !audioSource.isPlaying)
                {
                    audioSource.time = audioSource.clip.length * Random.value;
                    audioSource.Play();
                }
                if (!audible && audioSource.isPlaying)
                    audioSource.Pause(); //perhaps Stop()?
            }
        }

        [System.Serializable]
        public class FrictionSound : Sound
        {
            //Fields
            [Min(0)]
            [Space(10)]
            public float maxForce = 10000;

            [Header("Amounts")]
            public float rollingAmount = 1;
            public float slidingAmount = 1;

            [Header("Rates")]
            public float clipChangeSmoothTime = 0.001f;
            [Tooltip("This is used in smoothing the volume and pitch")]
            public SmoothTimes volumeSmoothTimes = SmoothTimes.Default();
            public float pitchSmoothTime = 0.025f;
            [Space(20)]
            [Min(0)]
            public float frictionNormalForceMultiplier = 0.1f;

            internal Source[] sources;



            //Datatypes
            [System.Serializable]
            public struct SmoothTimes
            {
                public float up;
                public float down;

                public static SmoothTimes Default()
                {
                    return new SmoothTimes()
                    {
                        up = 0.05f,
                        down = .15f
                    };
                }
            }

            internal class Source : LoopSource
            {
                public bool given;
                public SurfaceTypeSounds.Clip clip;



                //Methods
                public bool Silent => !Audible(currentVolume);

                public void ChangeClip(SurfaceTypeSounds.Clip clip, CollisionEffects cs)
                {
                    if (this.clip != clip)
                    {
                        //.25
                        //.5

                        //if (cs.impactCooldownT <= 0)
                        //{
                        //    cs.impactCooldownT = cs.impactCooldown;

                        //    var ftci = cs.frictionTypeChangeImpact;

                        //    var isases = cs.impactSound.audioSources;
                        //    var isas = isases[Random.Range(0, isases.Length)]; //Gets random one, because it's not very elegant to find which source otherwise

                        //    if (Audible(currentVolume) && Random.value < ftci.probability)
                        //        sts.PlayOneShot(isas, ftci.volumeMultiplier * currentVolume, ftci.pitchMultiplier * currentPitch);
                        //}

                        this.clip = clip;
                    }
                }

                protected static void SmoothDamp(ref float value, float target, ref float velocity, SmoothTimes rates) //float 
                {
                    float smoothTime;
                    if (target > value)
                        smoothTime = rates.up;
                    else
                        smoothTime = rates.down;

                    float maxChange = Time.deltaTime * smoothTime;

                    var wantedChange = target - value;
                    //var clampedChange = Mathf.Clamp(wantedChange, -maxChange, maxChange);
                    //value += clampedChange;

                    var before = value;
                    value = Mathf.SmoothDamp(value, target, ref velocity, smoothTime);
                    float clampedChange = value - before;

                    //if (wantedChange == 0)
                    //    return 1;
                    //else
                    //    return clampedChange / wantedChange; //returns the amount it has lerped, basically what the t would be in a Mathf.Lerp(value, target, t);
                }

                public void Update(FrictionSound fs, float totalVolumeMultiplier, float totalPitchMultiplier, float force, float speed)
                {
                    force = Mathf.Min(force, fs.maxForce);

                    void SetVolume()
                    {
                        audioSource.volume = Mathf.Min(totalVolumeMultiplier * currentVolume, fs.maxVolume * clip.volumeMultiplier);
                    }

                    if (clip == null)
                        return;

                    float targetPitch = clip.pitchMultiplier * fs.Pitch(speed);

                    if (audioSource.clip != clip.clip)
                    {
                        //Changes the clip if silent
                        if (!Audible(currentVolume))
                        {
                            audioSource.clip = clip.clip;
                            currentPitch = targetPitch; //Immediately changes the pitch
                            volumeVelocity = pitchVelocity = 0;
                        }

                        //Fades the volume to change the clip
                        SmoothDamp(ref currentVolume, 0, ref volumeVelocity, new SmoothTimes() { down = fs.clipChangeSmoothTime });
                        SetVolume();
                    }
                    else
                    {
                        if (audioSource.clip == null)
                        {
                            if (audioSource.isPlaying)
                                audioSource.Stop();
                        }
                        else
                        {
                            //Smoothly fades the pitch and volume
                            SmoothDamp(ref currentVolume, clip.volumeMultiplier * fs.Volume(force), ref volumeVelocity, fs.volumeSmoothTimes); //float lerpedAmount = 
                            SetVolume();

                            if (speed != 0)
                                currentPitch = Mathf.SmoothDamp(currentPitch, targetPitch, ref pitchVelocity, fs.pitchSmoothTime); // Mathf.LerpUnclamped(currentPitch, targetPitch, lerpedAmount);

                            audioSource.pitch = totalPitchMultiplier * currentPitch;

                            EnsurePlayingOnlyIfAudible();
                        }
                    }
                }
            }
        }
    }
}

/*
 * 
            //[Header("Force")]
            //public float minForce = 1;
            //public float maxForce = 100;

 */
//private struct CollisionSound
//{
//    public float force, speed;
//    public SurfaceSoundSet.SurfaceTypeSounds s;
//}

//[System.Serializable]
//public class FrictionTypeChangeImpact //Just for foldering
//{
//    [Range(0, 1)]
//    public float probability = 1;
//    public float volumeMultiplier = 1;
//    public float pitchMultiplier = 1;
//}