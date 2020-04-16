/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

//Blends are used to give a material more than one surface type (similar to Terrain's alphaMap)

namespace PrecisionSurfaceEffects
{
    [System.Serializable]
    public class BlendOverride : MarkerOverride
    {
        public SurfaceBlends blends = new SurfaceBlends();
    }

    [AddComponentMenu("PSE/Surface Markers/Surface Blend-Overrides Marker", -1000)]
    public sealed class SurfaceBlendOverridesMarker : OverridesMarker<BlendOverride>
    {
        protected override void Refresh(BlendOverride sm)
        {
            sm.blends.SortNormalize();
        }
    }
}