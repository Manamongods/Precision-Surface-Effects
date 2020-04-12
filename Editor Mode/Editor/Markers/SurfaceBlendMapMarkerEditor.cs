using UnityEngine;
using UnityEditor;
using PrecisionSurfaceEffects;

[CustomEditor(typeof(SurfaceBlendMapMarker))]
[CanEditMultipleObjects]
public class SurfaceBlendMapMarkerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var s = target as SurfaceBlendMapMarker;

        if (s.GetComponent<MeshCollider>().convex)
            EditorGUILayout.HelpBox("The MeshCollider is Convex", MessageType.Error);
        if (!s.GetComponent<MeshFilter>().sharedMesh.isReadable)
            EditorGUILayout.HelpBox("The Mesh is not set to Readable", MessageType.Error);
        for (int i = 0; i < s.blendMaps.Length; i++)
        {
            var bm = s.blendMaps[i];
            var map = bm.map;
            if(!map.isReadable)
                EditorGUILayout.HelpBox("The AlphaMap Texture: \"" + map.name + "\" is not set to Readable", MessageType.Error);

            void Warn(SurfaceBlendMapMarker.BlendMap.SurfaceBlends2 sb2)
            {
                if(sb2.colorMap != null && !sb2.colorMap.isReadable)
                    EditorGUILayout.HelpBox("The Color Texture: \"" + sb2.colorMap.name + "\" is not set to Readable", MessageType.Error);
            }

            Warn(bm.r);
            Warn(bm.g);
            Warn(bm.b);
            Warn(bm.a);
        }

        base.OnInspectorGUI();
    }
}