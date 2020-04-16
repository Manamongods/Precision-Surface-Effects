/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Malee;

//If your game reuses materials you can easily use these

namespace PrecisionSurfaceEffects
{
    [CreateAssetMenu(menuName = "Precision Surface Effects/Terrain Blends")]
    internal class MaterialBlendOverrides : GroupBlends<MaterialBlendOverrideGroup, Material> { }

    [System.Serializable]
    internal class MaterialBlendOverrideGroup : BlendGroup<Material>
    {
        [Space(10)]
        public Material[] materials = new Material[1];

        internal override Material[] GetKeys => materials;
    }
}