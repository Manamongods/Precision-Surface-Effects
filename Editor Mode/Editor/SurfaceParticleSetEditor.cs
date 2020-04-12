//using UnityEngine;
//using UnityEditor;
//using System.Collections;
//using Malee.Editor;
//using System;
//using PrecisionSurfaceEffects;

//[CanEditMultipleObjects]
//[CustomEditor(typeof(SurfaceParticleSet))]
//public class SurfaceParticleSetEditor : Editor
//{
//    private ReorderableList surfaceTypeParticles; //private SerializedProperty list2; //private ReorderableList list3;

//    void OnEnable()
//    {
//        surfaceTypeParticles = new ReorderableList(serializedObject.FindProperty("surfaceTypeParticles"));
//        surfaceTypeParticles.elementNameProperty = "myEnum";

//        //list2 = serializedObject.FindProperty("list2");

//        //list3 = new ReorderableList(serializedObject.FindProperty("list3"));
//        //list3.getElementNameCallback += GetList3ElementName;
//    }

//    //private string GetList3ElementName(SerializedProperty element)
//    //{
//    //    return element.propertyPath;
//    //}

//    public override void OnInspectorGUI()
//    {
//        serializedObject.Update();

//        //draw the list using GUILayout, you can of course specify your own position and label
//        surfaceTypeParticles.DoLayoutList();

//        ////Caching the property is recommended
//        //EditorGUILayout.PropertyField(list2);

//        ////draw the final list, the element name is supplied through the callback defined above "GetList3ElementName"
//        //list3.DoLayoutList();

//        ////Draw without caching property
//        //EditorGUILayout.PropertyField(serializedObject.FindProperty("list4"));
//        //EditorGUILayout.PropertyField(serializedObject.FindProperty("list5"));

//        //serializedObject.ApplyModifiedProperties();
//    }
//}