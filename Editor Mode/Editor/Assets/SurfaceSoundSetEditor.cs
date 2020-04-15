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
    private SerializedProperty data, testLoopVolumeMultiplier;
    private ReorderableList surfaceTypeSounds; 

    void OnEnable()
    {
        surfaceTypeSounds = new ReorderableList(serializedObject.FindProperty("surfaceTypeSounds"));

        data = serializedObject.FindProperty("data");
        testLoopVolumeMultiplier = serializedObject.FindProperty("testLoopVolumeMultiplier");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(data);
        EditorGUILayout.Space(30);
        surfaceTypeSounds.DoLayoutList();
        EditorGUILayout.Space(30);
        EditorGUILayout.PropertyField(testLoopVolumeMultiplier);

        serializedObject.ApplyModifiedProperties();
    }
}