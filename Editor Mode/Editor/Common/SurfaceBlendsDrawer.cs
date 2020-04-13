using UnityEngine;
using UnityEditor;

namespace PrecisionSurfaceEffects
{
    //[CustomPropertyDrawer(typeof(SurfaceBlendMapMarker.BlendMap.SurfaceBlends2))]
    //public class SurfaceBlends2Drawer : SurfaceBlendsDrawer { }

    [CustomPropertyDrawer(typeof(SurfaceBlends), true)]
    public class SurfaceBlendsDrawer : PropertyDrawer
    {
        public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
        {
            return EditorGUI.GetPropertyHeight(property, true);
        }

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            string name = property.FindPropertyRelative("groupName").stringValue;
            if(label.text != name)
                label.text = label.text + " - " + name;
            EditorGUI.PropertyField(position, property, label, true);

            EditorGUI.EndProperty();
        }
    }
}