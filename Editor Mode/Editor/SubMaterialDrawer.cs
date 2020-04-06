using UnityEngine;
using UnityEditor;

namespace PrecisionSurfaceEffects
{
    [CustomPropertyDrawer(typeof(SubMaterial))]
    public class SubMaterialDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            const float mid = 0.4f;

            var indexRect = position;
            indexRect.width *= mid;
            EditorGUI.PropertyField(indexRect, property.FindPropertyRelative("materialID"), GUIContent.none);

            var materialRect = position;
            materialRect.width *= 1 - mid;
            materialRect.x += mid * position.width;
            EditorGUI.PropertyField(materialRect, property.FindPropertyRelative("material"), GUIContent.none);

            EditorGUI.EndProperty();
        }
    }
}