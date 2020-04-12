using UnityEngine;
using UnityEditor;
using System.Collections;
using Malee.Editor;
using System;
using PrecisionSurfaceEffects;

[CanEditMultipleObjects]
[CustomEditor(typeof(SurfaceSoundSet))]
public class SurfaceSoundSettEditor : Editor
{
    private SerializedProperty data;
    private ReorderableList surfaceTypeSounds; 

    void OnEnable()
    {
        surfaceTypeSounds = new ReorderableList(serializedObject.FindProperty("surfaceTypeSounds"));

        data = serializedObject.FindProperty("data");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(data);
        EditorGUILayout.Space(30);
        surfaceTypeSounds.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}