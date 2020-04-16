/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [DisallowMultipleComponent]
    [AddComponentMenu("PSE/Surface Markers/Surface Type Marker", -1000)]
    public class SurfaceTypeMarker : SingleMarker
    {
        //Fields
        public string reference = "Grass";
        [HideInInspector]
        [SerializeField]
        internal string lowerReference;


        //Methods
        public override void Refresh()
        {
            base.Refresh();
            lowerReference = reference.ToLowerInvariant();
        }


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