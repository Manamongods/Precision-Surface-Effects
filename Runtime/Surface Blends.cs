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
using UnityExtensions; // using System.Linq;

namespace PrecisionSurfaceEffects
{
    [System.Serializable]
    internal class NormalizedBlend
    {
        public string reference;
        public float normalizedWeight;
        public float volume;
        public float pitch;

        internal int surfaceTypeID; //only used for terrains
    }

    [System.Serializable]
    public class SurfaceBlends //internal
    {
#if UNITY_EDITOR
        public string groupName; //header

        [SeperatorLine]
        public float volumeMultiplier = 1;
        public float pitchMultiplier = 1;

        [Tooltip("These are used for multiple surface type weights")]
        [Space(10)]
        public SurfaceBlend[] blends = new SurfaceBlend[1] { new SurfaceBlend() };

        [System.Serializable]
        public class SurfaceBlend
        {
            public string reference = "Grass";
            [Min(0)]
            public float weight = 1;

#if UNITY_EDITOR
            [SerializeField]
            [HideInInspector]
            internal float normalizedWeight = 1;
#endif
        }

        public void SortNormalize()
        {
            float weightSum = 0;
            for (int i = 0; i < blends.Length; i++)
                weightSum += blends[i].weight;

            result.Clear();
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
                };

                result.Add(br);
            }

            result.Sort((x, y) => y.normalizedWeight.CompareTo(x.normalizedWeight)); //Descending
        }
#endif

        [HideInInspector]
        [SerializeField]
        internal List<NormalizedBlend> result = new List<NormalizedBlend>();
    }
}