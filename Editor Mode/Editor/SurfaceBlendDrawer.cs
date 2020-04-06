using UnityEngine;
using UnityEditor;

namespace PrecisionSurfaceEffects
{
    [CustomPropertyDrawer(typeof(SurfaceBlends.Blend))]
    public class SurfaceBlendDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            // Using BeginProperty / EndProperty on the parent property means that
            // prefab override logic works on the entire property.
            EditorGUI.BeginProperty(position, label, property);

            // Draw label
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);
            //position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), GUIContent.none);

            // Don't make child fields be indented
            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            //EditorGUI.indentLevel = 3;


            const float outer = 0.25f;
            position.x += position.width * outer;
            position.width *= (1 - outer);


            const float mid = 0.6f;
            const float mid2 = 0.8f;

            var referenceRect = position;
            referenceRect.width *= mid;
            EditorGUI.PropertyField(referenceRect, property.FindPropertyRelative("reference"), GUIContent.none);

            var weightRect = position;
            weightRect.width *= mid2 - mid;
            weightRect.x += mid * position.width;
            EditorGUI.PropertyField(weightRect, property.FindPropertyRelative("weight"), GUIContent.none);

            var normalizedWeightRect = position;
            normalizedWeightRect.width *= 1 - mid2;
            normalizedWeightRect.x += mid2 * position.width;
            GUI.enabled = false;

            float nw = property.FindPropertyRelative("normalizedWeight").floatValue;
            var content = new GUIContent(nw.ToString("0.000"), "Normalized Weight");
            EditorGUI.PrefixLabel(normalizedWeightRect, GUIUtility.GetControlID(FocusType.Passive), content);

            // Set indent back to what it was
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}