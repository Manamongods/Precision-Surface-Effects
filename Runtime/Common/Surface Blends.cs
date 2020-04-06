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

        [SeperatorLine]
        public float hardnessMultiplier = 1;

        [Header("Sound")]
        public float volumeMultiplier = 1;
        public float pitchMultiplier = 1;

        [Header("Particles")]
        public float particleSizeMultiplier = 1; //public float particleSpeedMultiplier = 1;

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

                var br = new NormalizedBlend()
                {
                    reference = blend.reference,

                    normalizedWeight = weight,
                    volume = volumeMultiplier,
                    pitch = pitchMultiplier,
                    hardness = hardnessMultiplier,
                    particleSize = particleSizeMultiplier, //particleSpeed = particleSpeedMultiplier,
                    tintColor = blend.tintColor,
                };

                result.result.Add(br);
            }

            result.result.Sort((x, y) => y.normalizedWeight.CompareTo(x.normalizedWeight)); //Descending
        }



        //Datatypes
        [System.Serializable]
        public class Blend
        {
            public string reference = "Grass";
            [Min(0)]
            public float weight = 1;
            public Color tintColor = Color.white; //if there is a marker override

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
            public string reference;
            public int surfaceTypeID;

            public float normalizedWeight;
            public float hardness;

            public float volume;
            public float pitch;

            public Color tintColor; //This is per blend
            public Color baseColor; //This is per keyword/color override markers (individual ones or one). It will go for the specific and then the total.
            public float particleSize; //public float particleSpeed;
        }
    }
}