using UnityEngine;
using UnityEditor;

namespace PrecisionSurfaceEffects
{
    [CustomPropertyDrawer(typeof(ParticleMultipliers))]
    public class ParticleMultipliersDrawer : PropertyDrawer
    {
        const float total = 2.5f;
        const float actual = 2;
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUIUtility.singleLineHeight * total;
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            position.y += (total - actual) * 0.5f / total * position.height;
            position.height /= total;

            EditorGUI.BeginProperty(position, label, property);


            position = EditorGUI.PrefixLabel(position, label);


            //const float mid = 0.25f;
            //position.x += position.width * mid;
            //position.width *= 1 - mid;

            EditorGUI.PropertyField(position, property.FindPropertyRelative("countMultiplier"));
            position.y += position.height;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("sizeMultiplier"));


            EditorGUI.EndProperty();
        }
    }
}