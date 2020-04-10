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
using UnityEditor;
using UnityEngine;
using UnityExtensions;

//This contains the identification of a surface type

namespace PrecisionSurfaceEffects
{
    [System.Serializable]
    public class STSettings
    {
        public float hardness = 1;
        public Color defaultColor = Color.white;
        public float volumeMultiplier = 1;
        public float pitchMultiplier = 1;
    }

    [CreateAssetMenu(menuName = "Precision Surface Effects/Surface Type")]
    public class SurfaceType : ScriptableObject
    {
        public float hardnessMultiplier = 1;
        public Color defaultColorTint = Color.white;

        [Space(30)]
        public SubType[] subTypes = new SubType[1] { new SubType() };

        [System.Serializable]
        public class SubType
        {
            public string keyword = "Grass"; //public string[] keywords = new string[1] { "Grass" };
            [SerializeField]
            [HideInInspector]
            internal string lowerKeyword = "grass";

            public STSettings settings = new STSettings();
        }

        private void OnValidate()
        {
            for (int i = 0; i < subTypes.Length; i++)
            {
                var st = subTypes[i];
                st.lowerKeyword = st.keyword.ToLowerInvariant();
            }
        }
    }
}