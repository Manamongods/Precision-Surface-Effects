/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [DisallowMultipleComponent]
    public class SurfaceTypeMarker : SingleMarker
    {
        public string reference = "Grass";
        internal string lowerReference;

        public void Refresh()
        {
            lowerReference = reference.ToLowerInvariant();
        }

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            Refresh();
        }
#endif
    }
}