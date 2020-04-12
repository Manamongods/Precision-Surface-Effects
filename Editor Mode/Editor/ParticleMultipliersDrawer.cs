//using UnityEngine;
//using UnityEditor;

//namespace PrecisionSurfaceEffects
//{
//    [CustomPropertyDrawer(typeof(ParticleMultipliers))]
//    public class ParticleMultipliersDrawer : PropertyDrawer
//    {
//        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
//        {
//            return EditorGUIUtility.singleLineHeight * 2;
//        }

//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            position.height *= 0.5f;

//            EditorGUI.BeginProperty(position, label, property);


//            position = EditorGUI.PrefixLabel(position, label);

//            var lw = EditorGUIUtility.labelWidth;
//            EditorGUIUtility.labelWidth *= 1.75f;

//            const float mid = 0.25f;
//            position.x += position.width * mid;
//            position.width *= 1 - mid;

//            EditorGUI.PropertyField(position, property.FindPropertyRelative("countMultiplier"));
//            position.y += position.height;
//            EditorGUI.PropertyField(position, property.FindPropertyRelative("sizeMultiplier"));

//            EditorGUIUtility.labelWidth = lw;


//            EditorGUI.EndProperty();
//        }
//    }
//}