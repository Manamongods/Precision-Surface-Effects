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
using System.Linq;

//Blends are used to give a material more than one surface type (similar to Terrain's alphaMap)

namespace PrecisionSurfaceEffects
{
    [DisallowMultipleComponent]
    public abstract class Marker : MonoBehaviour { }

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    public sealed class SurfaceBlendOverridesMarker : Marker
    {
        //[ReorderableList()]
        [SerializeField]
        internal SubMaterial[] subMaterials = new SubMaterial[1] { new SubMaterial() };

        [System.Serializable]
        internal class SubMaterial : SurfaceBlends
        {
            public int materialID;
            [ReadOnly]
            public Material material;
        }

#if UNITY_EDITOR
        private void OnValidate()
        {
            var mr = GetComponent<MeshRenderer>();

            for (int i = 0; i < subMaterials.Length; i++)
            {
                var sm = subMaterials[i];
                sm.SortNormalize();

                var mats = mr.sharedMaterials;
                sm.materialID = Mathf.Clamp(sm.materialID, 0, mats.Length - 1);
                sm.material = mats[sm.materialID];
            }
        }
#endif
    }
}