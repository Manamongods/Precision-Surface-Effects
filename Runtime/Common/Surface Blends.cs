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
using UnityExtensions;

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
                particleSize = blend.particleSizeMultiplier, //particleSpeed = particleSpeedMultiplier,
                particleCount = blend.particleCountMultiplier,
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
            public float particleSizeMultiplier = 1; //public float particleSpeedMultiplier = 1;
            public float particleCountMultiplier = 1;
            public Color color = Color.white;
            public SurfaceParticleOverrides particleOverrides;

#if UNITY_EDITOR
            [SerializeField]
            [HideInInspector]
            internal float normalizedWeight = 1;
#endif
        }

        internal class NormalizedBlends //BlendResult
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

            public float particleSize; 
            public float particleCount; 
        }
    }
}