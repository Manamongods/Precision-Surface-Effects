using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    public class BasicVibrationSound : VibrationSound
    {
        //Fields
        [Space(10)]
        public VibrationLoop vibrationSound = new VibrationLoop();



        //Methods
        public override void ResetSounds()
        {
            vibrationSound.impulse = 0;
            vibrationSound.weightedSpeed = 0;
        }

        protected override void AddVibration(Vector3 position, float impulse, float speed)
        {
            vibrationSound.Add(impulse, speed);
        }



        //Datatypes
        [System.Serializable]
        public class VibrationLoop : CollisionEffects.LoopSource
        {
            //Fields
            [Min(0)]
            [Header("Volume")]
            public float volumeByForce = 0.1f;
            [Header("Pitch")]
            public ScaledAnimationCurve pitchBySpeed = new ScaledAnimationCurve();
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
                    float targetPitch = pitchBySpeed.Evaluate(speed);

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



        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            vibrationSound.audioSource.loop = true;
            vibrationSound.audioSource.playOnAwake = false;
        }
#endif

        private void OnDisable()
        {
            vibrationSound.audioSource.Pause();
        }

        private void Update() //Turn this into a coroutine?
        {
            vibrationSound.Update(scaling.totalVolumeMultiplier, scaling.totalPitchMultiplier);
        }
    }
}