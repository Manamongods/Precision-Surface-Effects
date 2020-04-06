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

namespace PrecisionSurfaceEffects
{
    public partial class CollisionSounds : CollisionEffectsMaker
    {
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

        [System.Serializable]
        public class VibrationSound
        {
            public AudioSource audioSource;

            public float volumeByForce = 0.1f;

            public float basePitch = 0.5f;
            public float pitchBySpeed = 0.035f;
        }

        [System.Serializable]
        public class Sound
        {
            //Fields
            [Tooltip("The more, the more surface types can be heard at once")]
            public AudioSource[] audioSources;
            public float minimumTypeWeight = 0.2f; // minimumTypeForce = 0.2f; //any surface type contribution*forceSum lower than this will be ignored

            [Header("Volume")]
            public float volumeByForce = 0.1f; //public float baseVolume = 0;
            public Vector2 volumeFaderBySpeedRange = new Vector2(0.01f, 0.1f);

            [Header("Pitch")]
            public float basePitch = 0.5f;
            public float pitchBySpeed = 0.035f;

            //internal SurfaceTypeSounds[] stses;


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

        [System.Serializable]
        public class FrictionSound : Sound
        {
            //Fields
            [Header("Rates")]
            public float clipChangeSmoothTime = 0.001f;
            [Tooltip("This is used in smoothing the volume and pitch")]
            public SmoothTimes smoothTimes = SmoothTimes.Default(); //make it be smoothtime instead?

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

            internal class Source
            {
                //public SurfaceTypeSounds sts;
                public AudioSource audioSource;
                public bool given;
                public SurfaceTypeSounds.Clip clip;

                private float currentVolume;
                private float currentPitch;
                private float volumeVelocity;
                private float pitchVelocity;



                //Methods
                public bool Silent => !Audible(currentVolume);

                public void ChangeClip(SurfaceTypeSounds.Clip clip, CollisionSounds cs)
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

                public void Update(FrictionSound fs, float totalVolumeMultiplier, float totalPitchMultiplier, float force, float speed)
                {
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
                        audioSource.volume = currentVolume;
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
                            float lerpedAmount = SmoothDamp(ref currentVolume, clip.volumeMultiplier * fs.Volume(force), ref volumeVelocity, fs.smoothTimes);
                            audioSource.volume = totalVolumeMultiplier * currentVolume;

                            if (speed != 0)
                                SmoothDamp(ref currentPitch, targetPitch, ref pitchVelocity, fs.smoothTimes); // Mathf.LerpUnclamped(currentPitch, targetPitch, lerpedAmount);
                            audioSource.pitch = totalPitchMultiplier * currentPitch;


                            //Ensures the AudioSource is only playing if the volume is high enough
                            bool audible = Audible(currentVolume);
                            if (audible && !audioSource.isPlaying)
                                audioSource.Play();
                            if (!audible && audioSource.isPlaying)
                                audioSource.Pause(); //perhaps Stop()?
                        }
                    }
                }

                private static float SmoothDamp(ref float value, float target, ref float velocity, SmoothTimes rates)
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

                    if (wantedChange == 0)
                        return 1;
                    else
                        return clampedChange / wantedChange; //returns the amount it has lerped, basically what the t would be in a Mathf.Lerp(value, target, t);
                }
                private static bool Audible(float vol)
                {
                    return vol > 0.00000001f;
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