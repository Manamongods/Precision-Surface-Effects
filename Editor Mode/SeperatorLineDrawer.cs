
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[System.AttributeUsage(System.AttributeTargets.Field)]
public class SeperatorLineAttribute : MultiPropertyAttribute
{
    private bool under;
    public SeperatorLineAttribute(bool under = false)
    {
        this.under = under;
    }

#if UNITY_EDITOR
    public bool Under
    {
        get { return under; }
    }

    internal override float? GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true) * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var lineRect = position;
        lineRect.height *= 0.5f;
        var h = lineRect.height;

        var actualRect = lineRect;
        if (!Under)
            actualRect.y += h;
        else
            lineRect.y += h;

        base.OnGUI(actualRect, property, label);
        EditorGUI.LabelField(lineRect, "", GUI.skin.horizontalSlider);
    }
#endif
}

/*
 * 
public class UnderSeperatorLineAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(UnderSeperatorLineAttribute))]
internal sealed class UnderSeperatorLineDrawer : SeperatorLineDrawer
{
    protected override bool Under => true;
}

[CustomPropertyDrawer(typeof(SeperatorLineAttribute))]
internal class SeperatorLineDrawer : PropertyDrawer
{
    protected virtual bool Under => false;

    
}

 * public class SeperatorLineAttribute : PropertyAttribute { }
public class UnderSeperatorLineAttribute : PropertyAttribute { }

[CustomPropertyDrawer(typeof(UnderSeperatorLineAttribute))]
internal sealed class UnderSeperatorLineDrawer : SeperatorLineDrawer
{
    protected override bool Under => true;
}

[CustomPropertyDrawer(typeof(SeperatorLineAttribute))]
internal class SeperatorLineDrawer : PropertyDrawer
{
    protected virtual bool Under => false;

    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        return EditorGUI.GetPropertyHeight(property, label, true) * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        var rect = position;
        rect.height *= 0.5f;
        var h = rect.height;

        var actualRect = rect;
        if (!Under)
            actualRect.y += h;
        else
            rect.y += h;

        void Actual()
        {
            EditorGUI.PropertyField(actualRect, property, label, true);
        }

        if (!Under)
            Actual();

        EditorGUI.LabelField(rect, "", GUI.skin.horizontalSlider);

        if (Under)
            Actual();
    }
}
 * 
 * 
 * 
 *     
 */
