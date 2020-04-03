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
using UnityEditor;
using UnityEngine;
using UnityExtensions;

namespace PrecisionSurfaceEffects
{
    [CreateAssetMenu(menuName = "Precision Surface Effects/Surface Data")] 
    public partial class SurfaceData : ScriptableObject
    {
        //Fields
        [Space(20)]
        [Tooltip("If it can't find one")]
        public int defaultSurfaceType = 0;
#if UNITY_EDITOR
        public string autoDefaultSurfaceTypeGroupName;
#endif

        [Space(20)]
        [ReorderableList()]
        public SurfaceType[] surfaceTypes = new SurfaceType[1];

        [Header("Materials")]
        [Space(20)]
        [Tooltip("If your game reuses materials you can easily ")]
        [SerializeField]
        internal MaterialBlendOverride[] materialBlendOverrides = new MaterialBlendOverride[] { new MaterialBlendOverride() };

        [Header("Terrain Textures")]
        [Space(20)]
        [Tooltip("If your game reuses materials you can easily ")]
        [SerializeField]
        internal TerrainBlends[] terrainBlends = new TerrainBlends[] { new TerrainBlends() };


        private readonly Dictionary<Material, List<BlendResult>> materialBlendLookup = new Dictionary<Material, List<BlendResult>>(); //for faster lookup



        //Datatypes
        [System.Serializable]
        internal class TerrainBlends : SurfaceBlends
        {
            public Texture[] terrainAlbedos = new Texture[1];
        }

        [System.Serializable]
        internal class MaterialBlendOverride : SurfaceBlends
        {
            public Material[] materials = new Material[1];
        }



        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            defaultSurfaceType = Mathf.Clamp(defaultSurfaceType, 0, surfaceTypes.Length - 1);
            autoDefaultSurfaceTypeGroupName = surfaceTypes[defaultSurfaceType].name;

            Awake();
        }
#endif

        private void Awake()
        {
            materialBlendLookup.Clear();
            for (int i = 0; i < materialBlendOverrides.Length; i++)
            {
                var mbo = materialBlendOverrides[i];
                mbo.SortNormalize();

                for (int ii = 0; ii < mbo.materials.Length; ii++)
                {
                    var mat = mbo.materials[ii];
                    if(mat != null)
                        materialBlendLookup.Add(mat, mbo.result);
                }
            }
        }
    }
}

/*
 *         private int GetMainTexture(Terrain terrain, Vector3 WorldPos, out float mix, float totalMax)
        {
            // returns the zero-based index of the most dominant texture
            // on the main terrain at this world position.
            float[] mixes = GetTextureMix(terrain, WorldPos);

            return GetMainTexture(mixes, out float mix, totalMax);
        }

 * 
                for (int iii = 0; iii < ss.clipVariants.Length; iii++)
                {
                    var cv = ss.clipVariants[iii];

                    if (cv.probabilityWeight == 0)
                        cv.probabilityWeight = 1;
                }

private static readonly List<int> subMeshTriangles = new List<int>(); //to avoid some amount of constant reallocation

if (mesh.isReadable)
{
    //Much slower version. I don't know if the faster version will be consistent though, because I don't know how unity does things internally, so if there are problems then see if this fixes it. In my testing the faster version works fine though:
    int[] triangles = mesh.triangles;

    var triIndex = rh.triangleIndex * 3;
    int a = triangles[triIndex + 0], b = triangles[triIndex + 1], c = triangles[triIndex + 2];

    for (int submeshID = 0; submeshID < mesh.subMeshCount; submeshID++)
    {
        subMeshTriangles.Clear();
        mesh.GetTriangles(subMeshTriangles, submeshID);

        for (int i = 0; i < subMeshTriangles.Count; i += 3)
        {
            int aa = subMeshTriangles[i + 0], bb = subMeshTriangles[i + 1], cc = subMeshTriangles[i + 2];
            if (a == aa && b == bb && c == cc)
            {
                checkName = materials[submeshID].name; //the triangle hit is within this submesh

                goto Found; //This exits the nested loop, to avoid any more comparisons (for performance)
            }
        }
    }
}

                    //Found:





#if UNITY_EDITOR
            [UnityEditor.CustomPropertyDrawer(typeof(Clip))]
            public class ClipDrawer : UnityEditor.PropertyDrawer
            {
                public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
                {
                    return UnityEditor.EditorGUIUtility.singleLineHeight;
                }

                public override void OnGUI(Rect rect, SerializedProperty property, GUIContent label)
                {
                    var pw = property.FindPropertyRelative("probabilityWeight");
                    var c = property.FindPropertyRelative("clip");

                    var r = rect;
                    r.width *= 0.5f;
                    UnityEditor.EditorGUI.PropertyField(r, property, false);

                    r = rect;
                    r.width *= 0.5f;
                    UnityEditor.EditorGUI.PropertyField(r, pw);

                    r.x += r.width;
                    UnityEditor.EditorGUI.PropertyField(r, c, null as GUIContent);
                }

                private void OnEnable()
                {

                }
            }

#endif
*/