using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class ReadOnlyAttribute : MultiPropertyAttribute
{
#if UNITY_EDITOR
    internal override void OnPreGUI(Rect position, SerializedProperty property)
    {
        GUI.enabled = false;
    }
    internal override void OnPostGUI(Rect position, SerializedProperty property)
    {
        GUI.enabled = true;
    }
#endif
}