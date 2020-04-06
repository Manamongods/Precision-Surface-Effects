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
using UnityEngine;
using UnityExtensions;
using System.Linq;

//The weights are normalized in runtime
//now I have to getcomponent both though
//find the barycentric coordinates? 
//has to be readable
//make it use list for uv instead?

namespace PrecisionSurfaceEffects
{
    //This gives a smooth control similar to Terrain, to MeshRenderers 

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    [DisallowMultipleComponent]
    public sealed class SurfaceBlendMapMarker : Marker
    {
        //Fields
        [SerializeField]
        internal BlendMap[] blendMaps = new BlendMap[1] { new BlendMap() };

        private Vector3[] vertices;
        private int[] triangles;

        private static readonly List<Vector2> temporaryUVs = new List<Vector2>();



        //Methods
        private void Add(SurfaceData sd, SurfaceBlends.NormalizedBlends blendResults, float weightMultiplier, ref float totalWeight)
        {
            for (int i = 0; i < blendResults.result.Count; i++)
            {
                var blend = blendResults.result[i];

                sd.AddBlend(blend, null, null, true, weightMultiplier, ref totalWeight);
            }
        }

        internal bool TryAddBlends(SurfaceData sd, Mesh mesh, int submeshID, Vector3 point, int triangleID, out float totalWeight)
        {
            totalWeight = 0;

            bool success = false;

            var triangle = triangleID * 3;
            var t0 = triangles[triangle + 0];
            var t1 = triangles[triangle + 1];
            var t2 = triangles[triangle + 2];
            var a = vertices[t0];
            var b = vertices[t1];
            var c = vertices[t2];
            var bary = new Barycentric(a, b, c, point);

            float totalTotalWeight = 0;
            for (int i = 0; i < blendMaps.Length; i++)
                totalTotalWeight += blendMaps[i].weight;

            for (int i = 0; i < blendMaps.Length; i++)
            {
                var bm = blendMaps[i];

                for (int ii = 0; ii < bm.subMaterials.Length; ii++)
                {
                    if (bm.subMaterials[ii].materialID == submeshID)
                    {
                        var uv = bary.Interpolate(bm.uvs[t0], bm.uvs[t1], bm.uvs[t2]);
                        uv = uv * new Vector2(bm.st.x, bm.st.y) + new Vector2(bm.st.z, bm.st.w); //?
                        var color = bm.map.GetPixelBilinear(uv.x, uv.y); //this only works for clamp or repeat btw (not mirror etc.)

                        float rgbaSum = color.r + color.g + color.b + color.a;
                        float invTotal = bm.weight / (rgbaSum * totalTotalWeight);

                        Add(sd, bm.r.result, color.r * invTotal, ref totalWeight);
                        Add(sd, bm.g.result, color.g * invTotal, ref totalWeight);
                        Add(sd, bm.b.result, color.b * invTotal, ref totalWeight);
                        Add(sd, bm.a.result, color.a * invTotal, ref totalWeight);

                        success = true;
                        break;
                    }
                }
            }

            return success;
        }

       

        //Datatypes
        [System.Serializable]
        internal class BlendMap
        {
            //Fields
            public float weight = 1;

            [SerializeField]
            internal SubMaterial[] subMaterials = new SubMaterial[1] { new SubMaterial() };

            [Min(0)]
            public int uvID = 0;
            public Texture2D map; //must be readable
            public Vector4 st = new Vector4(1, 1, 0, 0);

            public SurfaceBlends r = new SurfaceBlends();
            public SurfaceBlends g = new SurfaceBlends();
            public SurfaceBlends b = new SurfaceBlends();
            public SurfaceBlends a = new SurfaceBlends();

            public Vector2[] uvs;

            internal Color sample;



            //Datatypes
            [System.Serializable]
            public class SubMaterial
            {
                public int materialID;
                [ReadOnly]
                public Material material;
            }
        }



        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            var mr = GetComponent<MeshRenderer>();
            var mats = mr.sharedMaterials;

            for (int i = 0; i < blendMaps.Length; i++)
            {
                var bm = blendMaps[i];
                bm.r.SortNormalize();
                bm.g.SortNormalize();
                bm.b.SortNormalize();
                bm.a.SortNormalize();

                for (int ii = 0; ii < bm.subMaterials.Length; ii++)
                {
                    var sm = bm.subMaterials[ii];

                    sm.materialID = Mathf.Clamp(sm.materialID, 0, mats.Length - 1);
                    sm.material = mats[sm.materialID];
                }
            }
        }

        private void Awake()
        {
            var mf = GetComponent<MeshFilter>();
            var m = mf.sharedMesh;

            vertices = m.vertices;
            triangles= m.triangles;
            
            for (int i = 0; i < blendMaps.Length; i++)
			{
                var bm = blendMaps[i];
                temporaryUVs.Clear();
                m.GetUVs(bm.uvID, temporaryUVs);
                bm.uvs = temporaryUVs.ToArray();
            }
        }
#endif
    }
}