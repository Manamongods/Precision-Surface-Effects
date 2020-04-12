using UnityEngine;
using UnityEditor;
using System.Collections;
using Malee.Editor;
using System;
using PrecisionSurfaceEffects;

[CanEditMultipleObjects]
[CustomEditor(typeof(SurfaceParticleSet))]
public class SurfaceParticleSetEditor : Editor
{
    private SerializedProperty data;
    private ReorderableList surfaceTypeParticles; 

    void OnEnable()
    {
        surfaceTypeParticles = new ReorderableList(serializedObject.FindProperty("surfaceTypeParticles"));

        data = serializedObject.FindProperty("data");
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();

        EditorGUILayout.PropertyField(data);
        EditorGUILayout.Space(30);
        surfaceTypeParticles.DoLayoutList();

        serializedObject.ApplyModifiedProperties();
    }
}