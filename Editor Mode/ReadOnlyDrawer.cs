#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class ReadOnlyAttribute : MultiPropertyAttribute
{
    internal override void OnPreGUI(Rect position, SerializedProperty property)
    {
        GUI.enabled = false;
    }
    internal override void OnPostGUI(Rect position, SerializedProperty property)
    {
        GUI.enabled = true;
    }
}
#endif