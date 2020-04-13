/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#if UNITY_EDITOR
using UnityEditor;
using Malee.Editor;
using System;
using PrecisionSurfaceEffects;
#endif

namespace PrecisionSurfaceEffects
{
#if UNITY_EDITOR
    [CustomEditor(typeof(TerrainBlends))]
    public class TerrainBlendsEditor : GroupBlendsEditor { }

    [CustomEditor(typeof(MaterialBlendOverrides))]
    public class MaterialBlendOverridesEditor : GroupBlendsEditor { }

    [CanEditMultipleObjects]
    public abstract class GroupBlendsEditor : Editor
    {
        private ReorderableList surfaceTypeParticles;

        void OnEnable()
        {
            surfaceTypeParticles = new ReorderableList(serializedObject.FindProperty("groups"));
        }

        public override void OnInspectorGUI()
        {
            serializedObject.Update();
            surfaceTypeParticles.DoLayoutList();
            serializedObject.ApplyModifiedProperties();
        }
    }
#endif


    [System.Serializable]
    internal abstract class BlendGroup<T> : SurfaceBlends
    {
        internal abstract T[] GetKeys { get; }
    }

    internal class GroupBlends<T, TT> : ScriptableObject where T : BlendGroup<TT>, new()
    {
        //Fields
        [SerializeField]
        internal T[] groups = new T[] { new T() };
    }


    [CreateAssetMenu(menuName = "Precision Surface Effects/Terrain Blends")]
    internal class TerrainBlends : GroupBlends<TerrainBlendGroup, Texture> { }

    [System.Serializable]
    internal class TerrainBlendGroup : BlendGroup<Texture>
    {
        [Space(10)]
        public Texture[] terrainAlbedos = new Texture[1];

        internal override Texture[] GetKeys => terrainAlbedos;
    }
}