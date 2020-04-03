using UnityEngine;
using UnityEditor;
using PrecisionSurfaceEffects;

[CustomEditor(typeof(SurfaceBlendOverridesMarker))]
[CanEditMultipleObjects]
public class SurfaceBlendOverridesMarkerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var s = target as SurfaceBlendOverridesMarker;

        if(s.GetComponent<MeshCollider>().convex)
        EditorGUILayout.HelpBox("The MeshCollider is Convex", MessageType.Warning);

        base.OnInspectorGUI();
    }
}