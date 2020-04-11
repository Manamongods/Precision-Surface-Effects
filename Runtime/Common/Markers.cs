/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [RequireComponent(typeof(Collider))]
    public abstract class Marker : MonoBehaviour
    {
        [HideInInspector]
        public Marker[] markerFamily;

        public bool GetMarker<MarkerType>(out MarkerType marker) where MarkerType : Marker
        {
            //marker = GetComponent<MarkerType>(); //See how badly this performs lol
            //return marker;

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