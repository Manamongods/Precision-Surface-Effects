using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//Untested

public class CollisionSounds : SurfaceSoundsUser
{
    //Fields
    public float volumeMultiplier = 1;

    public Sound impactSound = new Sound();
    public FrictionSound frictionSound = new FrictionSound();

    private float force, speed;



    //Methods
    private SurfaceSounds.SurfaceType.SoundSet GetSoundSet(Collision c)
    {
        return surfaceSounds.GetCollisionSurfaceType(c).GetSoundSet(soundSetID);
    }



    //Datatypes
    [System.Serializable]
    public class Sound
    {
        //Fields
        public AudioSource audioSource;

        [Header("Volume")]
        public float baseVolume = 0;
        public float volumeByForce;

        [Header("Pitch")]
        public float basePitch = 1;
        public float pitchBySpeed;


        //Methods
        public float Volume(float force)
        {
            return baseVolume + volumeByForce * force;
        }

        public float Pitch(float speed)
        {
            return basePitch + pitchBySpeed * speed;
        }
    }

    [System.Serializable]
    public class FrictionSound : Sound
    {
        //Fields
        [Header("Rates")]
        public float clipChangeLerpRate = 100;
        public float volumeLerpRate = 10;
        public float pitchLerpRate = 10;

        private float volume;
        private float pitch;

        internal SurfaceSounds.SurfaceType.SoundSet.Clip ssClip;

        
        //Methods
        public void Update(float volumeMultiplier, float force, float speed)
        {
            float targetPitch = ssClip.pitchMultiplier * Pitch(speed);

            if (audioSource.clip != ssClip.clip)
            {
                //Changes the clip if silent
                if (!Audible(volume))
                {
                    audioSource.clip = ssClip.clip;
                    pitch = targetPitch; //Immediately changes the pitch
                }

                //Fades the volume to change the clip
                LerpTo(ref volume, 0, clipChangeLerpRate);
                audioSource.volume = volume;
            }
            else
            {
                //Smoothly fades the pitch and volume
                LerpTo(ref volume, ssClip.volumeMultiplier * volumeMultiplier * Volume(force), volumeLerpRate);
                audioSource.volume = volume;
                LerpTo(ref pitch, targetPitch, pitchLerpRate);
                audioSource.pitch = pitch;

                //Ensures the AudioSource is only playing if the volume is high enough
                bool audible = Audible(volume);
                if (audible && !audioSource.isPlaying)
                    audioSource.Play();
                if (!audible && audioSource.isPlaying)
                    audioSource.Pause(); //perhaps Stop()?
            }
        }
        private static void LerpTo(ref float value, float target, float rate)
        {
            float maxChange = Time.deltaTime * rate;
            value += Mathf.Clamp(target - value, -maxChange, maxChange);
        }
        private static bool Audible(float vol)
        {
            return vol > 0.00000001f;
        }
    }



    //Lifecycle
#if UNITY_EDITOR
    private void OnValidate()
    {
        impactSound.audioSource.loop = false;
        frictionSound.audioSource.loop = true;
    }
#endif

    private void FixedUpdate()
    {
        //Clears these accumulations
        force = 0;
        speed = 0;
    }

    private void OnCollisionEnter(Collision collision)
    {
        //Impact Sound
        var vol = volumeMultiplier * impactSound.Volume(collision.impulse.magnitude); //Here "force" is actually an impulse
        var pitch = impactSound.Pitch(collision.relativeVelocity.magnitude);

        GetSoundSet(collision).PlayOneShot(impactSound.audioSource, vol, pitch);
    }
    private void OnCollisionStay(Collision collision)
    {
        //Friction Sound
        var force = collision.impulse.magnitude / Time.deltaTime;
        var speed = collision.relativeVelocity.magnitude;

        force += force;
        speed += force * speed; //weights speed, so that it can find a weighted average pitch for all the potential OnCollisionStays

        frictionSound.ssClip = GetSoundSet(collision).loopSound;
    }

    private void Update()
    {
        float speed = 0;
        if (force > 0) //prevents a divide by zero
            speed = this.speed / force;

        frictionSound.Update(volumeMultiplier, force, speed);
    }
}
