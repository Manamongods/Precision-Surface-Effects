/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [System.Serializable]
    public class SurfaceBlends
    {
        //Fields
        public string groupName; //header

        [Tooltip("These are used for multiple surface type weights")]
        [Header("Blends")]
        [Space(10)]
        public Blend[] blends = new Blend[1] { new Blend() };

        [HideInInspector]
        [SerializeField]
        internal NormalizedBlends result = new NormalizedBlends();



        //Methods
        public void SortNormalize()
        {
            float weightSum = 0;
            for (int i = 0; i < blends.Length; i++)
                weightSum += blends[i].weight;

            result.result.Clear();
            for (int i = 0; i < blends.Length; i++)
            {
                var blend = blends[i];

                var weight = blend.weight / weightSum;
#if UNITY_EDITOR
                blend.normalizedWeight = weight;
#endif

                result.result.Add(GetNormalized(blend, weight));
            }

            result.result.Sort((x, y) => y.normalizedWeight.CompareTo(x.normalizedWeight)); //Descending
        }
        internal static NormalizedBlend GetNormalized(Blend blend, float weight)
        {
            return new NormalizedBlend()
            {
                normalizedWeight = weight,

                lowerReference = blend.reference.ToLowerInvariant(),
                volume = blend.volumeMultiplier,
                pitch = blend.pitchMultiplier,
                hardness = blend.hardnessMultiplier,
                selfParticleMultipliers = blend.selfParticleMultipliers, //particleSpeed = particleSpeedMultiplier,
                otherParticleMultipliers = blend.otherParticleMultipliers,
                color = blend.color,
                particleOverrides = blend.particleOverrides
            };
        }



        //Datatypes
        [System.Serializable]
        public class Blend
        {
            public string reference = "Grass";
            [Min(0)]
            public float weight = 1;

            [SeperatorLine]
            public float hardnessMultiplier = 1;

            [Header("Sound")]
            public float volumeMultiplier = 1;
            public float pitchMultiplier = 1;

            [Header("Particles")]
            public ParticleMultipliers selfParticleMultipliers = ParticleMultipliers.Default();
            public ParticleMultipliers otherParticleMultipliers = ParticleMultipliers.Default();
            public Color color = Color.white;
            public SurfaceParticleOverrides particleOverrides;

#if UNITY_EDITOR
            [SerializeField]
            [HideInInspector]
            internal float normalizedWeight = 1;
#endif
        }

        [System.Serializable]
        internal sealed class NormalizedBlends
        {
            public List<NormalizedBlend> result = new List<NormalizedBlend>();
        }

        [System.Serializable]
        internal struct NormalizedBlend
        {
            public string lowerReference;
            public int surfaceTypeID;

            public float normalizedWeight;
            public float hardness;

            public float volume;
            public float pitch;

            public Color color;
            public SurfaceParticleOverrides particleOverrides;
            public ParticleMultipliers selfParticleMultipliers;
            public ParticleMultipliers otherParticleMultipliers;
        }
    }
}