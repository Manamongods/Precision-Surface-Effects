using UnityEngine;
using UnityEditor;
using System.Collections;
using Malee.Editor;
using System;
using PrecisionSurfaceEffects;

[CanEditMultipleObjects]
[CustomEditor(typeof(SurfaceData))]
public class SurfaceDataEditor : Editor
{
    private ReorderableList surfaceTypes, materialBlendOverrides, terrainBlends;

    void OnEnable()
    {
        surfaceTypes = new ReorderableList(serializedObject.FindProperty("surfaceTypes"));
        materialBlendOverrides = new ReorderableList(serializedObject.FindProperty("materialBlendOverrides"));
        terrainBlends = new ReorderableList(serializedObject.FindProperty("terrainBlends"));
    }

    public override void OnInspectorGUI()
    {
        serializedObject.Update();


        var bold = EditorStyles.boldLabel;


        EditorGUILayout.Space(20);
        EditorGUILayout.LabelField("Surface Types", bold);
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultSurfaceType"));
        GUI.enabled = false;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultSurfaceTypeGroupName"));
        GUI.enabled = true;
        EditorGUILayout.PropertyField(serializedObject.FindProperty("defaultSurfaceTypeSettings"));

        EditorGUILayout.Space(20);
        surfaceTypes.DoLayoutList();

        EditorGUILayout.Space(40);
        EditorGUILayout.LabelField("Materials", bold);
        materialBlendOverrides.DoLayoutList();

        EditorGUILayout.Space(40);
        EditorGUILayout.LabelField("Terrain Textures", bold);
        terrainBlends.DoLayoutList();


        serializedObject.ApplyModifiedProperties();
    }
}