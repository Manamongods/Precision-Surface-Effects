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
        //Fields
        [HideInInspector]
        public Marker[] markerFamily;
        [HideInInspector]
        public MeshRenderer meshRenderer;
        [HideInInspector]
        public bool hasMR;
        [HideInInspector]
        public MeshFilter meshFilter;


        //Methods
        public static bool GetMR(bool anyMarkers, Marker marker, GameObject gameObject, out MeshRenderer mr)
        {
            if (anyMarkers)
            {
                mr = marker.meshRenderer;
                return marker.hasMR;
            }
            mr = gameObject.GetComponent<MeshRenderer>();
            return !object.Equals(mr, null);//?
        }
        public static MeshFilter GetMF(bool anyMarkers, Marker marker, GameObject gameObject)
        {
            if (anyMarkers)
                return marker.meshFilter;
            return gameObject.GetComponent<MeshFilter>();
        }

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

        public virtual void Refresh()
        {
            markerFamily = GetComponents<Marker>();
            meshFilter = GetComponent<MeshFilter>();
            meshRenderer = GetComponent<MeshRenderer>();
            hasMR = meshRenderer != null;
        }


        //Lifecycle
#if UNITY_EDITOR
        protected virtual void OnValidate()
        {
            Refresh();
        }
#endif
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
        public SubmeshType[] overrides = new SubmeshType[1] { new SubmeshType() };


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

        public override void Refresh()
        {
            base.Refresh();

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