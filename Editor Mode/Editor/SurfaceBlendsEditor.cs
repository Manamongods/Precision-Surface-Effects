//using UnityEngine;
//using UnityEditor;

//namespace PrecisionSurfaceEffects
//{
//    [CustomPropertyDrawer(typeof(SurfaceBlends), true)]
//    [CanEditMultipleObjects]
//    public class SurfaceBlendsDrawer : PropertyDrawer
//    {
//        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
//        {
//            EditorGUI.BeginProperty(position, label, property);


//            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

//            float height = position.y;

//            EditorGUI.PropertyField(position, property.FindPropertyRelative("header"), GUIContent.none);
//            position.y += height;
//            EditorGUI.PropertyField(position, property.FindPropertyRelative("header"), GUIContent.none);
//            //EditorGUILayout.PropertyField(property.FindPropertyRelative("header"), GUIContent.none);



//            EditorGUI.EndProperty();
//        }
//    }
//}