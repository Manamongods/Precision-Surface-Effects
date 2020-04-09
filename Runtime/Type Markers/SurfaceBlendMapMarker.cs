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

//This gives a smooth control similar to Terrain, to MeshRenderers 

namespace PrecisionSurfaceEffects
{
    [System.Serializable]
    public class SubMaterial
    {
        public int materialID;
        [ReadOnly]
        public Material material;
    }

    [RequireComponent(typeof(MeshRenderer), typeof(MeshFilter), typeof(MeshCollider))]
    [DisallowMultipleComponent]
    public sealed class SurfaceBlendMapMarker : Marker
    {
        //Fields
#if UNITY_EDITOR
        [ReadOnly]
        [SerializeField]
        private List<Color> lastSampledColors = new List<Color>();
#endif

        [Space(30)]
        [SerializeField]
        public BlendMap[] blendMaps = new BlendMap[1] { new BlendMap() };

        private Vector3[] vertices;
        private int[] triangles;

        private static readonly List<Vector2> temporaryUVs = new List<Vector2>();



        //Methods
        private void Add(SurfaceData sd, SurfaceBlends.NormalizedBlends blendResults, float weightMultiplier, ref float totalWeight)
        {
            if (weightMultiplier <= 0.0000000001f)
                return;

            for (int i = 0; i < blendResults.result.Count; i++)
            {
                var blend = blendResults.result[i];

                sd.AddBlend(blend, false, weightMultiplier, ref totalWeight);
            }
        }

        internal bool TryAddBlends(SurfaceData sd, Mesh mesh, int submeshID, Vector3 point, int triangleID, out float totalWeight)
        {
            totalWeight = 0;

            //Finds Barycentric
            var triangle = triangleID * 3;
            var t0 = triangles[triangle + 0];
            var t1 = triangles[triangle + 1];
            var t2 = triangles[triangle + 2];
            var a = vertices[t0];
            var b = vertices[t1];
            var c = vertices[t2];
            point = transform.InverseTransformPoint(point);
            var bary = new Barycentric(a, b, c, point);

#if UNITY_EDITOR
            lastSampledColors.Clear();
#endif

            float totalTotalWeight = 0;
            for (int i = 0; i < blendMaps.Length; i++)
            {
                var bm = blendMaps[i];
                bm.sampled = false;

                for (int ii = 0; ii < bm.subMaterials.Length; ii++)
                {
                    if (bm.subMaterials[ii].materialID == submeshID)
                    {
                        var uv = bary.Interpolate(bm.uvs[t0], bm.uvs[t1], bm.uvs[t2]);
                        uv = uv * new Vector2(bm.uvScaleOffset.x, bm.uvScaleOffset.y) + new Vector2(bm.uvScaleOffset.z, bm.uvScaleOffset.w); //?

                        Color color = bm.map.GetPixelBilinear(uv.x, uv.y); //this only works for clamp or repeat btw (not mirror etc.)
                        bm.sampledColor = color;

                        totalTotalWeight += bm.weight * (color.r + color.g + color.b + color.a);

#if UNITY_EDITOR
                        lastSampledColors.Add(color);
#endif

                        bm.sampled = true;
                        break;
                    }
                }
            }

            if (totalTotalWeight > 0)
            {
                float invTotalTotal = 1f / totalTotalWeight;

                for (int i = 0; i < blendMaps.Length; i++)
                {
                    var bm = blendMaps[i];

                    if (bm.sampled)
                    {
                        float invTotal = bm.weight * invTotalTotal;

                        var color = bm.sampledColor;
                        Add(sd, bm.r.result, color.r * invTotal, ref totalWeight);
                        Add(sd, bm.g.result, color.g * invTotal, ref totalWeight);
                        Add(sd, bm.b.result, color.b * invTotal, ref totalWeight);
                        Add(sd, bm.a.result, color.a * invTotal, ref totalWeight);
                    }
                }

                return true;
            }

            return false;
        }

        public void Refresh()
        {
            for (int i = 0; i < blendMaps.Length; i++)
            {
                var bm = blendMaps[i];
                bm.r.SortNormalize();
                bm.g.SortNormalize();
                bm.b.SortNormalize();
                bm.a.SortNormalize();
            }
        }

        

        //Datatypes
        [System.Serializable]
        public class BlendMap
        {
            //Fields
            public float weight = 1;

            [Header("Materials")]
            [SerializeField]
            public SubMaterial[] subMaterials = new SubMaterial[1] { new SubMaterial() };

            [Header("Texture")]
            public Texture2D map; //must be readable
            [Range(0, 7)]
            public int uvChannel = 0;
            public Vector4 uvScaleOffset = new Vector4(1, 1, 0, 0); //st

            [Header("Channel Blends")]
            public SurfaceBlends r = new SurfaceBlends();
            public SurfaceBlends g = new SurfaceBlends();
            public SurfaceBlends b = new SurfaceBlends();
            public SurfaceBlends a = new SurfaceBlends();

            internal Vector2[] uvs;

            internal bool sampled;
            internal Color sampledColor;
        }



        //Lifecycle
#if UNITY_EDITOR
        protected override void OnValidate()
        {
            base.OnValidate();

            lastSampledColors.Clear();

            Refresh();

            Awake();
        }
#endif

        private void Awake()
        {
            var mf = GetComponent<MeshFilter>();
            var m = mf.sharedMesh;

            vertices = m.vertices;
            triangles = m.triangles;
            
            for (int i = 0; i < blendMaps.Length; i++)
			{
                var bm = blendMaps[i];
                temporaryUVs.Clear();
                m.GetUVs(bm.uvChannel, temporaryUVs);
                bm.uvs = temporaryUVs.ToArray();
            }

            var mr = GetComponent<MeshRenderer>();
            var mats = mr.sharedMaterials;

            for (int i = 0; i < blendMaps.Length; i++)
            {
                var bm = blendMaps[i];

                for (int ii = 0; ii < bm.subMaterials.Length; ii++)
                {
                    var sm = bm.subMaterials[ii];

                    sm.materialID = Mathf.Clamp(sm.materialID, 0, mats.Length - 1);
                    sm.material = mats[sm.materialID];
                }
            }
        }
    }
}

/*
 * 

            //var mf = GetComponent<MeshFilter>();
            //var m = mf.sharedMesh;

            //for (int i = 0; i < blendMaps.Length; i++)
            //{
            //    var bm = blendMaps[i];
            //    bm.uvChannel = Mathf.Min(bm.uvChannel, m.)
            //}

*/