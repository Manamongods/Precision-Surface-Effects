#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class Indent : MultiPropertyAttribute
{
    internal override void OnPreGUI(Rect position, SerializedProperty property)
    {
        EditorGUI.indentLevel++;
    }
    internal override void OnPostGUI(Rect position, SerializedProperty property)
    {
        EditorGUI.indentLevel--;
    }
}
#endif