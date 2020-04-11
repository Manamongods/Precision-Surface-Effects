/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;

namespace PrecisionSurfaceEffects
{
    //[System.Serializable]
    //public class SurfaceParticlesSettings
    //{

    //}

    [CreateAssetMenu(menuName = "Precision Surface Effects/Surface Sound Set")]
    public class SurfaceSoundSet : SurfaceSet<SurfaceTypeSounds>
    {
        //Fields
        [Space(30)]
        [ReorderableList()]
        //[UnityEngine.Serialization.FormerlySerializedAs("surfaceTypes")]
        public SurfaceTypeSounds[] surfaceTypeSounds = new SurfaceTypeSounds[] { new SurfaceTypeSounds() };


        //Methods
        public void PlayOneShot(SurfaceOutput output, AudioSource audioSource, float volumeMultiplier = 1, float pitchMultiplier = 1)
        {
            var vol = volumeMultiplier * output.volumeMultiplier * output.weight;
            var pitch = pitchMultiplier * output.pitchMultiplier;
            surfaceTypeSounds[output.surfaceTypeID].PlayOneShot(audioSource, vol, pitch);
        }


        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            Resize(ref surfaceTypeSounds);
        }
#endif
    }

    [System.Serializable]
    public class SurfaceTypeSounds : SurfaceSetType
    {
        //Fields
        [Header("Volume")]
        public float volume = 1;
        [Range(0, 1)]
        public float volumeVariation = 0.2f;

        [Header("Pitch")]
        public float pitch = 1;
        [Range(0, 1)]
        public float pitchVariation = 0.2f;

        [Header("Clips")]
        public ShotClip[] clipVariants = new ShotClip[1] { new ShotClip() };

        [Header("Friction/Rolling")]
        [Space(20)]
        public Clip frictionSound = new Clip(); //(no randomization should be used for this clip)


        //Datatypes
        [System.Serializable]
        public class Clip
        {
            public AudioClip clip;

            public float volumeMultiplier = 1;
            public float pitchMultiplier = 1;
        }

        [System.Serializable]
        public class ShotClip : Clip
        {
            [Min(0)]
            public float probabilityWeight = 1; //normalized for clipVariants
        }


        //Methods
        public void PlayOneShot(AudioSource audioSource, float volumeMultiplier = 1, float pitchMultiplier = 1)
        {
            var c = GetRandomClip(out float volume, out float pitch);

            if (c != null)
            {
                //if(!source.isPlaying)
                audioSource.pitch = pitch * pitchMultiplier;

                audioSource.PlayOneShot(c, volume * volumeMultiplier);
            }
        }

        public AudioClip GetRandomClip(out float volume, out float pitch)
        {
            volume = GetVolume();
            pitch = GetPitch();

            var c = GetRandomClip();
            if (c != null)
            {
                volume *= c.volumeMultiplier;
                pitch *= c.pitchMultiplier;

                return c.clip;
            }

            return null;
        }

        private float GetVolume()
        {
            return volume * (1 + (Random.value - 0.5f) * volumeVariation);
        }
        private float GetPitch()
        {
            return pitch * (1 + (Random.value - 0.5f) * pitchVariation);
        }
        private Clip GetRandomClip()
        {
            float totalWeight = 0;
            for (int i = 0; i < clipVariants.Length; i++)
                totalWeight += clipVariants[i].probabilityWeight;

            float rand = Random.value * totalWeight;
            float finder = 0f;
            for (int i = 0; i < clipVariants.Length; i++)
            {
                var cv = clipVariants[i];
                finder += cv.probabilityWeight;
                if (finder >= rand - 0.000000001f) //I just do that just in case of rounding errors (i dunno)
                    return cv;
            }

            return null;
        }
    }
}