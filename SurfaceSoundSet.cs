using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;

namespace SurfaceSounds
{
    [CreateAssetMenu(menuName = "SurfaceSoundSet")]
    public class SurfaceSoundSet : ScriptableObject
    {
        //Fields
        public SurfaceTypes surfaceTypes;
        [ReorderableList()]
        public SurfaceTypeSounds[] surfaceTypeSounds = new SurfaceTypeSounds[] { new SurfaceTypeSounds() };



        //Datatypes
        [System.Serializable]
        public class SurfaceTypeSounds
        {
            //Fields
            public string autoGroupName = "";

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

            [Header("Friction/Rolling if wanted")]
            [Tooltip("This can be used for friction/rolling sounds, or just ignore it")]
            [Space(20)]
            public Clip loopSound = new Clip(); //(no randomization should be used for this clip)


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
                    if (finder >= rand - 0.000000001f) //I just do that just in case of rounding errors (idk)
                        return cv;
                }

                return null;
            }
        }



        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (surfaceTypes == null)
                return;

            var l = surfaceTypes.surfaceTypes.Length;
            if (l > surfaceTypeSounds.Length)
                System.Array.Resize(ref surfaceTypeSounds, l);

            for (int i = 0; i < surfaceTypes.surfaceTypes.Length; i++)
            {
                surfaceTypeSounds[i].autoGroupName = surfaceTypes.surfaceTypes[i].groupName;
            }
        }
#endif
    }
}