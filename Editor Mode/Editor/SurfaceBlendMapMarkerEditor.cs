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
            var map = s.blendMaps[i].map;
            if(!map.isReadable)
                EditorGUILayout.HelpBox("The Texture: \"" + map.name + "\" is not set to Readable", MessageType.Error);
        }

        base.OnInspectorGUI();
    }
}