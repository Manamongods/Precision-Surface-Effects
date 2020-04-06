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

namespace PrecisionSurfaceEffects
{
    [RequireComponent(typeof(Collider))]
    public abstract class Marker : MonoBehaviour
    {
        public Marker[] markerFamily;

        public bool GetMarker<MarkerType>(out MarkerType marker) where MarkerType : Marker
        {
            for (int i = 0; i < markerFamily.Length; i++)
            {
                if(markerFamily[i] is MarkerType m)
                {
                    marker = m;
                    return true;
                }
            }

            marker = default;
            return false;
        }

        protected virtual void OnValidate()
        {
            markerFamily = GetComponents<Marker>();
        }
    }

    public abstract class MarkerOverride
    {
        public int materialID;
        [ReadOnly]
        public Material material;
    }

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    [DisallowMultipleComponent]
    public abstract class OverridesMarker<SubmeshType> : Marker where SubmeshType : MarkerOverride, new()
    {
        //Fields
        [SerializeField]
        public SubmeshType[] overrides = new SubmeshType[1] { new SubmeshType() }; //submeshes //[ReorderableList()] //subMaterials


        //Methods
        public bool GetOverride(int submeshID, out SubmeshType o)
        {
            for (int i = 0; i < overrides.Length; i++)
            {
                var oi = overrides[i];
                if (oi.materialID == submeshID)
                {
                    o = oi;
                    return true;
                }
            }

            o = default;
            return false;
        }

        public void Refresh()
        {
            var mr = GetComponent<MeshRenderer>();
            var mats = mr.sharedMaterials;

            for (int i = 0; i < overrides.Length; i++)
            {
                var sm = overrides[i];
                sm.materialID = Mathf.Clamp(sm.materialID, 0, mats.Length - 1);
                sm.material = mats[sm.materialID];
                Refresh(sm);
            }
        }

        protected virtual void Refresh(SubmeshType submesh) { }


        //Lifecycle
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();
            Refresh();
        }
#endif
    }
}