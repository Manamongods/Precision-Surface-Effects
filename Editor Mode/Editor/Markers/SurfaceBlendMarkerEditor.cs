using UnityEngine;
using UnityEditor;
using PrecisionSurfaceEffects;

[CustomEditor(typeof(SurfaceBlendMarker))]
[CanEditMultipleObjects]
public class SurfaceBlendMarkerEditor : Editor
{
    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        var blends = serializedObject.FindProperty("blends");

        EditorGUILayout.PropertyField(blends.FindPropertyRelative("blends"));

        serializedObject.ApplyModifiedProperties();
    }
}