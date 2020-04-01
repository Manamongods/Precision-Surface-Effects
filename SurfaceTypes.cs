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

namespace SurfaceSounds
{
    [CreateAssetMenu(menuName = "Surface Sounds/Surface Types")]
    public class SurfaceTypes : ScriptableObject
    {
        //Fields
        [Space(20)]
        [Tooltip("If it can't find one")]
        public int defaultSurfaceType = 0;
#if UNITY_EDITOR
        public string autoDefaultSurfaceTypeHeader;
#endif

        [Space(20)]
        [ReorderableList()]
        public SurfaceType[] surfaceTypes = new SurfaceType[] { new SurfaceType() };



        //Methods
        public int GetSphereCastSurfaceTypeID(Vector3 worldPosition, Vector3 downDirection, float radius = 1, float maxDistance = Mathf.Infinity, int layerMask = -1)
        {
            if (Physics.SphereCast(worldPosition, radius, downDirection, out RaycastHit rh, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
#if UNITY_EDITOR
                var bottomCenter = worldPosition + downDirection * rh.distance;
                Debug.DrawLine(worldPosition, bottomCenter);
                Debug.DrawLine(bottomCenter, rh.point);
#endif

                return GetSurfaceType(rh.collider, worldPosition, rh.triangleIndex);
            }

            return GetSurfaceType(null, worldPosition);
        }
        public int GetRaycastSurfaceTypeID(Vector3 worldPosition, Vector3 downDirection, float maxDistance = Mathf.Infinity, int layerMask = -1)
        {
            if (Physics.Raycast(worldPosition, downDirection, out RaycastHit rh, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
#if UNITY_EDITOR
                Debug.DrawLine(worldPosition, rh.point);
#endif

                return GetSurfaceType(rh.collider, worldPosition, rh.triangleIndex);
            }

            return GetSurfaceType(null, worldPosition);
        }
        public int GetCollisionSurfaceTypeID(Collision collision)
        {
            return GetSurfaceType(collision.collider, collision.GetContact(0).point);
        }

        public bool TryGetStringSurfaceType(string checkName, out int stID)
        {
            if (!System.String.IsNullOrEmpty(checkName))
            {
                checkName = checkName.ToLowerInvariant();

                for (int i = 0; i < surfaceTypes.Length; i++)
                {
                    stID = i;
                    var st = surfaceTypes[i];

                    for (int ii = 0; ii < st.materialKeywords.Length; ii++)
                    {
                        if (checkName.Contains(st.materialKeywords[ii].ToLowerInvariant())) //check if the material name contains the keyword
                            return true;
                    }
                }
            }

            stID = -1;
            return false;
        }

        public int GetSurfaceType(Collider collider, Vector3 worldPosition, int triangleIndex = -1)
        {
            if (collider != null)
            {
                if (collider is TerrainCollider tc) //it is a terrain collider
                {
                    if (TryGetTerrainSurfaceType(tc.GetComponent<Terrain>(), worldPosition, out int stID))
                        return stID;
                }
                else
                {
                    if (TryGetNonTerrainSurfaceType(collider, worldPosition, out int stID, triangleIndex))
                        return stID;
                }
            }

            return defaultSurfaceType;
        }

        //You can make these public if you want access:
        private bool TryGetTerrainSurfaceType(Terrain terrain, Vector3 worldPosition, out int stID)
        {
            var terrainIndex = GetMainTexture(terrain, worldPosition);
            var terrainTextureName = terrain.terrainData.terrainLayers[terrainIndex].diffuseTexture.name; //This might be terrible performance??

            for (int i = 0; i < surfaceTypes.Length; i++)
            {
                stID = i;
                var st = surfaceTypes[i];

                for (int ii = 0; ii < st.terrainAlbedos.Length; ii++)
                {
                    if (terrainTextureName == st.terrainAlbedos[ii].name)
                    {
                        return true;
                    }
                }
            }

            stID = -1;
            return false;
        }
        private bool TryGetNonTerrainSurfaceType(Collider collider, Vector3 worldPosition, out int stID, int triangleIndex = -1)
        {
            //Finds CheckName
            string checkName = null;

            var marker = collider.GetComponent<SurfaceTypeMarker>();
            if (marker != null)
                checkName = marker.reference;

            var mr = collider.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                //Defaults to the first material. For most colliders it can't be discerned which specific material it is
                if (checkName == null) //It is overrided by SurfaceTypeMarker
                    checkName = mr.sharedMaterial.name;

                //The collider is a non-convex meshCollider. We can find the triangle index.
                if (triangleIndex != -1 && collider is MeshCollider mc && !mc.convex)
                {
                    var materials = mr.sharedMaterials;

                    var mesh = collider.GetComponent<MeshFilter>().sharedMesh;

                    var triIndex = triangleIndex * 3;

                    for (int submeshID = mesh.subMeshCount - 1; submeshID >= 0; submeshID--)
                    {
                        int start = mesh.GetSubMesh(submeshID).indexStart;

                        if (triIndex >= start)
                        {
                            //The triangle hit is within this submesh
                            checkName = materials[submeshID].name; //This is not overrided by SurfaceTypeMarker (which is useful because Collision sounds should have control over the default)
                            break;
                        }
                    }
                }
            }


            //Searches using CheckName
            if (TryGetStringSurfaceType(checkName, out stID))
                return true;

            //Found nothing
            stID = -1;
            return false;
        }


        //From: https://answers.unity.com/questions/456973/getting-the-texture-of-a-certain-point-on-terrain.html
        private float[] GetTextureMix(Terrain terrain, Vector3 WorldPos)
        {
            var terrainData = terrain.terrainData; //terrain = Terrain.activeTerrain;
            var terrainPos = terrain.transform.position;

            // returns an array containing the relative mix of textures
            // on the main terrain at this world position.

            // The number of values in the array will equal the number
            // of textures added to the terrain.

            // calculate which splat map cell the worldPos falls within (ignoring y)
            int mapX = (int)(((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
            int mapZ = (int)(((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);

            // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            float[,,] splatmapData = terrainData.GetAlphamaps(mapX, mapZ, 1, 1);

            // extract the 3D array data to a 1D array:
            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

            for (int n = 0; n < cellMix.Length; n++)
            {
                cellMix[n] = splatmapData[0, 0, n];
            }
            return cellMix;
        }
        private int GetMainTexture(Terrain terrain, Vector3 WorldPos)
        {
            // returns the zero-based index of the most dominant texture
            // on the main terrain at this world position.
            float[] mix = GetTextureMix(terrain, WorldPos);

            float maxMix = 0;
            int maxIndex = 0;

            // loop through each mix value and find the maximum
            for (int n = 0; n < mix.Length; n++)
            {
                if (mix[n] > maxMix)
                {
                    maxIndex = n;
                    maxMix = mix[n];
                }
            }
            return maxIndex;
        }



        //Datatypes
        [System.Serializable]
        public class SurfaceType
        {
            //Fields
            public string groupName = "Grassy Sound";

            [Header("Terrains")]
            public Texture2D[] terrainAlbedos;

            [Header("Mesh Renderers")]
            public string[] materialKeywords = new string[] { "Grass", "Leaves", "Hay", "Flower" };
        }



        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            defaultSurfaceType = Mathf.Clamp(defaultSurfaceType, 0, surfaceTypes.Length - 1);
            autoDefaultSurfaceTypeHeader = surfaceTypes[defaultSurfaceType].groupName;
        }
#endif
    }
}

/*
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
