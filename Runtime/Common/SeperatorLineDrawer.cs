#if UNITY_EDITOR
//From: https://answers.unity.com/questions/489942/how-to-make-a-readonly-property-in-inspector.html

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

public class SeperatorLineAttribute : PropertyAttribute
{

}

[CustomPropertyDrawer(typeof(SeperatorLineAttribute))]
public class SeperatorLineDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true) * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rect = position;
        rect.height *= 0.5f;

        var actualRect = rect;
        actualRect.y += actualRect.height;

        EditorGUI.PropertyField(actualRect, property, label, true);
        EditorGUI.LabelField(rect, "", GUI.skin.horizontalSlider);
    }
}
#endif