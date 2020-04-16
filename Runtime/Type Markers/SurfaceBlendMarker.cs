/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [DisallowMultipleComponent]
    public abstract class SingleMarker : Marker { }

    [RequireComponent(typeof(Collider))]
    [AddComponentMenu("PSE/Surface Markers/Surface Blend Marker", -1000)]
    public sealed class SurfaceBlendMarker : SingleMarker
    {
        [SerializeField]
        internal SurfaceBlends blends = new SurfaceBlends();

#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            blends.SortNormalize();
        }
#endif
    }
}