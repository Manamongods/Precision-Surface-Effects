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
    public partial class SurfaceData : ScriptableObject
    {
        //Constants
        private static readonly Color gizmoColor = Color.white * 0.75f;


        //Fields
        private static readonly List<Material> materials = new List<Material>();
        internal static readonly SurfaceOutputs outputs = new SurfaceOutputs();


        //Methods
        public SurfaceOutputs GetRHSurfaceTypes(RaycastHit rh, bool shareList = false)
        {
            outputs.Clear();
            outputs.hardness = 0;
            FillOutputs(rh);
            AddSurfaceTypes(rh.collider, rh.point, triangleIndex: rh.triangleIndex);
            return GetList(shareList);
        }
        public SurfaceOutputs GetSphereCastSurfaceTypes(Vector3 worldPosition, Vector3 downDirection, float radius, float maxDistance = Mathf.Infinity, int layerMask = -1, bool shareList = false)
        {
            PrepareOutputs(worldPosition);

            if (Physics.SphereCast(worldPosition, radius, downDirection, out RaycastHit rh, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
#if UNITY_EDITOR
                var bottomCenter = worldPosition + downDirection * rh.distance;
                Debug.DrawLine(worldPosition, bottomCenter, gizmoColor);
                Debug.DrawLine(bottomCenter, rh.point, gizmoColor);
#endif
                FillOutputs(rh);

                AddSurfaceTypes(rh.collider, rh.point, triangleIndex: rh.triangleIndex);
            }

            return GetList(shareList);
        }
        public SurfaceOutputs GetRaycastSurfaceTypes(Vector3 worldPosition, Vector3 downDirection, float maxDistance = Mathf.Infinity, int layerMask = -1, bool shareList = false)
        {
            PrepareOutputs(worldPosition);

            if (Physics.Raycast(worldPosition, downDirection, out RaycastHit rh, maxDistance, layerMask, QueryTriggerInteraction.Ignore))
            {
#if UNITY_EDITOR
                Debug.DrawLine(worldPosition, rh.point, gizmoColor);
#endif
                FillOutputs(rh);

                AddSurfaceTypes(rh.collider, rh.point, triangleIndex: rh.triangleIndex);
            }

            return GetList(shareList);
        }
        public SurfaceOutputs GetCollisionSurfaceTypes(Collision collision, bool shareList = false)
        {
            var con = collision.GetContact(0);
            var pos = con.point;

            PrepareOutputs(pos);
            outputs.collider = collision.collider;
            outputs.hitNormal = con.normal; //?

            AddSurfaceTypes(collision.collider, pos);

            return GetList(shareList);
        }

        private void PrepareOutputs(Vector3 worldPosition)
        {
            outputs.Clear();
            outputs.hardness = 0;
            outputs.collider = null;
            outputs.hitPosition = worldPosition;
            outputs.hitNormal = Vector3.zero;
        }
        private void FillOutputs(RaycastHit rh)
        {
            outputs.collider = rh.collider;
            outputs.hitPosition = rh.point;
            outputs.hitNormal = rh.normal;
        }

        private void NormalizeOutputs(float totalWeight)
        {
            if (totalWeight > 0)
                outputs.hardness /= totalWeight;

            for (int i = 0; i < outputs.Count; i++)
            {
                var o = outputs[i];
                if (o.weight > 0)
                {
                    float invW = 1f / o.weight;
                    o.volume *= invW;
                    o.pitch *= invW;
                    o.color *= invW;
                    o.particleSize *= invW;
                }
                outputs[i] = o;
            }
        }
        private SurfaceOutputs GetList(bool share)
        {
            if (share)
                return outputs;
            else
            {
                return new SurfaceOutputs(outputs)
                {
                    hardness = outputs.hardness,
                    collider = outputs.collider,
                    hitNormal = outputs.hitNormal,
                    hitPosition = outputs.hitPosition,
                };
            }
        }

        internal void AddSurfaceTypes(Collider collider, Vector3 worldPosition, int triangleIndex = -1)
        {
            if (collider != null)
            {
                if (collider is TerrainCollider tc) //it is a terrain collider
                {
                    AddTerrainSurfaceTypes(tc.GetComponent<Terrain>(), worldPosition);
                }
                else
                {
                    AddNonTerrainSurfaceTypes(collider, worldPosition, triangleIndex: triangleIndex);
                }
            }
        }
        private void AddTerrainSurfaceTypes(Terrain terrain, Vector3 worldPosition)
        {
            float totalWeight = 0;

            var mix = Utility.GetTextureMix(terrain, worldPosition);
            var layers = terrain.terrainData.terrainLayers; //This might be terrible performance??

            for (int mixID = 0; mixID < mix.Length; mixID++)
            {
                float alpha = mix[mixID];
                if (alpha > 0.000000001f) //Mathf.Epsilon
                {
                    var terrainTexture = layers[mixID].diffuseTexture; //.name;

                    if (terrainAlbedoBlendLookup.TryGetValue(terrainTexture, out SurfaceBlends.NormalizedBlends result))
                    {
                        for (int blendID = 0; blendID < result.result.Count; blendID++)
                            AddBlend(result.result[blendID], true, alpha, ref totalWeight);
                    }
                    else
                        AddBlend(defaultBlend, true, alpha, ref totalWeight);
                }
            }

            outputs.SortDescending();
            NormalizeOutputs(totalWeight);
        }
        private void AddNonTerrainSurfaceTypes(Collider collider, Vector3 worldPosition, int triangleIndex = -1)
        {
            //Markers
            var marker = collider.GetComponent<Marker>();
            bool anyMarkers = marker != null;

            //MeshRenderers
            var mr = collider.GetComponent<MeshRenderer>();
            if (mr != null)
            {
                //The collider is a non-convex meshCollider. We can find the triangle index.
                if (triangleIndex != -1 && collider is MeshCollider mc && !mc.convex)
                {
                    var mesh = collider.GetComponent<MeshFilter>().sharedMesh; //could use the marker to find this faster
                    int subMeshID = Utility.GetSubmesh(mesh, triangleIndex);

                    SurfaceBlends.NormalizedBlends blendResults = null;
                    if (anyMarkers)
                    {
                        //Blend Map Overrides
                        if (marker.GetMarker(out SurfaceBlendMapMarker blendMap))
                        {
                            if(blendMap.TryAddBlends(this, mesh, subMeshID, worldPosition, triangleIndex, out float totalWeight))
                            {
                                outputs.SortDescending();
                                NormalizeOutputs(totalWeight);
                                return;
                            }
                        }

                        //Blend Overrides
                        if (blendResults == null && marker.GetMarker(out SurfaceBlendOverridesMarker blendOverrides))
                            if(blendOverrides.GetOverride(subMeshID, out BlendOverride o))
                                blendResults = o.blends.result;
                    }

                    if (blendResults == null)
                    {
                        //Gets Materials
                        materials.Clear();
                        mr.GetSharedMaterials(materials);

                        if(materialBlendLookup.TryGetValue(materials[subMeshID], out SurfaceBlends.NormalizedBlends value))
                            blendResults = value;
                    }

                    if (blendResults != null)
                    {
                        AddSingleBlends(blendResults); 
                        return;
                    }
                    else
                    {
                        //Gets Materials
                        materials.Clear();
                        mr.GetSharedMaterials(materials);

                        //Adds based on keywords
                        if (TryGetStringSurfaceType(materials[subMeshID].name, out int stID, out SurfaceType st, out SurfaceType.SubType subType))
                            AddSingleOutput(stID, st, subType);
                        return;
                    }
                }
            }

            //Single Markers
            if (anyMarkers)
            {
                if (marker.GetMarker(out SurfaceTypeMarker typeMarker))
                {
                    //Single Type Marker
                    if (TryGetStringSurfaceType(typeMarker.reference, out int stID, out SurfaceType st, out SurfaceType.SubType subType))
                        AddSingleOutput(stID, st, subType);
                    return;
                }
                else if (marker.GetMarker(out SurfaceBlendMarker blendMarker))
                {
                    //Single Blend Marker
                    AddSingleBlends(blendMarker.blends.result);
                    return;
                }
            }

            //Defaults to the first material (For most colliders it can't be discerned which specific material it is)
            if (mr != null)
            {
                var mat = mr.sharedMaterial;

                //Blend Lookup
                if (materialBlendLookup.TryGetValue(mat, out SurfaceBlends.NormalizedBlends value))
                {
                    AddSingleBlends(value);
                    return;
                }

                //Name
                if (TryGetStringSurfaceType(mat.name, out int stID, out SurfaceType st, out SurfaceType.SubType subType))
                    AddSingleOutput(stID, st, subType);
                return;
            }
        }

        private void AddSingleBlends(SurfaceBlends.NormalizedBlends blendResults)
        {
            float totalWeight = 0;

            for (int i = 0; i < blendResults.result.Count; i++) ////int blendCount = Mathf.Min(blendResults.result.Count, maxOutputCount + 1);
            {
                var blend = blendResults.result[i];

                AddBlend(blend, false, 1, ref totalWeight);
            }

            outputs.SortDescending();
            NormalizeOutputs(totalWeight);
        }
        internal void AddBlend(SurfaceBlends.NormalizedBlend blendResult, bool terrain, float weightMultiplier, ref float totalWeight)
        {
            if (!terrain)
                blendResult = Settingsify(blendResult);

            float weight = blendResult.normalizedWeight * weightMultiplier;

            bool success = false;
            for (int outputID = 0; outputID < outputs.Count; outputID++)
            {
                var output = outputs[outputID];
                if (output.surfaceTypeID == blendResult.surfaceTypeID && output.particlesOverride == blendResult.particlesOverride)
                {
                    output.weight += weight;
                    output.volume += weight * blendResult.volume;
                    output.pitch += weight * blendResult.pitch;
                    output.color += weight * blendResult.color;
                    output.particleSize += weight * blendResult.particleSize; //output.particleSpeed += weight * blendResult.particleSpeed;

                    outputs[outputID] = output;
                    success = true;
                    break;
                }
            }

            if (!success)
            {
                outputs.Add
                (
                    new SurfaceOutput()
                    {
                        surfaceTypeID = blendResult.surfaceTypeID,
                        weight = weight,
                        volume = weight * blendResult.volume,
                        pitch = weight * blendResult.pitch,
                        color = weight * blendResult.color,
                        particleSize = weight * blendResult.particleSize,
                        particlesOverride = blendResult.particlesOverride,
                    }
                );
            }

            totalWeight += weight;
            var st = surfaceTypes[blendResult.surfaceTypeID];
            outputs.hardness += weight * blendResult.hardness * st.hardnessMultiplier;
        }
        private void AddSingleOutput(int stID, SurfaceType st, SurfaceType.SubType subType)
        {
            outputs.hardness = st.hardnessMultiplier;
            outputs.Add
            (
                new SurfaceOutput()
                {
                    surfaceTypeID = stID,
                    weight = 1,
                    volume = subType.settings.volumeMultiplier,
                    pitch = subType.settings.pitchMultiplier,
                    particlesOverride = null,
                    color = st.defaultColorTint * subType.settings.defaultColor,
                }
            );
        }

        private SurfaceBlends.NormalizedBlend Settingsify(SurfaceBlends.NormalizedBlend blendResult)
        {
            Color defaultTint = Color.white;

            STSettings settings;
            if (TryGetStringSurfaceType(blendResult.reference, out int stID, out SurfaceType st, out SurfaceType.SubType stst))
            {
                blendResult.surfaceTypeID = stID;
                settings = stst.settings;
                defaultTint = st.defaultColorTint;
            }
            else
            {
                blendResult.surfaceTypeID = defaultSurfaceType;
                settings = defaultSurfaceTypeSettings;
            }

            blendResult.pitch *= settings.pitchMultiplier;
            blendResult.volume *= settings.volumeMultiplier;
            blendResult.hardness *= settings.hardness;

            return blendResult;
        }

        public bool TryGetStringSurfaceType(string checkName, out int stID, out SurfaceType st, out SurfaceType.SubType subType)
        {
            if (!System.String.IsNullOrEmpty(checkName))
            {
                checkName = checkName.ToLowerInvariant();

                for (int i = 0; i < surfaceTypes.Length; i++)
                {
                    stID = i;
                    st = surfaceTypes[i];

                    for (int ii = 0; ii < st.subTypes.Length; ii++)
                    {
                        var stst = st.subTypes[ii];
                        if (checkName.Contains(stst.lowerKeyword)) //check if the material name contains the keyword
                        {
                            subType = stst;
                            return true;
                        }
                    }
                }
            }

            stID = -1;
            st = null;
            subType = null;
            return false;
        }
    }
}

/*
 *             //if (outputs.Count == 0)
            //    AddSingleOutput(defaultSurfaceType);
     
 *         private void SortCullOutputs()
        {
            while (outputs.Count > maxOutputCount + 2)
                outputs.RemoveAt(maxOutputCount + 1);
        }

 *         private void AddBlends(SurfaceBlends.NormalizedBlends blendResults, int maxOutputCount)
        {
            make it find first if already exists

            //Adds the blends. If a blend's reference doesn't match any ST, then it will give it the default ST
            int blendCount = Mathf.Min(blendResults.result.Count, maxOutputCount + 1);
            for (int i = 0; i < blendCount; i++)
            {
                var blend = blendResults.result[i];

                if (!TryGetStringSurfaceType(blend.reference, out int stID, out SurfaceType.SubType subType))
                    stID = defaultSurfaceType;

                var st = surfaceTypes[stID];
                outputs.hardness += blend.normalizedWeight * blendResults.hardness * st.hardness;

                outputs.Add(new SurfaceOutput() { surfaceTypeID = stID, weight = blend.normalizedWeight, volume = blend.volume, pitch = blend.pitch });
            }
        }
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