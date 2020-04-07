using UnityEngine;
using UnityEditor;
using PrecisionSurfaceEffects;

[CustomEditor(typeof(CollisionEffects))]
[CanEditMultipleObjects]
public class CollisionEffectsEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var s = target as CollisionEffects;

        var c = s.GetComponent<Collider>();

        bool needsParent = c == null || (c.attachedRigidbody != null && c.attachedRigidbody.transform != c.transform);
        if(needsParent)
        {
            string need = "This GameObject cannot receive OnCollision events. You need to use a CollisionEffectsParent component on the Rigidbody's GameObject";

            if (c == null)
                EditorGUILayout.HelpBox(need, MessageType.Info);
            else if (c.attachedRigidbody.GetComponent<CollisionEffectsParent>() == null)
                EditorGUILayout.HelpBox(need, MessageType.Warning);

            EditorGUILayout.HelpBox("Only the CollisionEffectsParent's priority does anything", MessageType.Info);
        }

        base.OnInspectorGUI();
    }
}