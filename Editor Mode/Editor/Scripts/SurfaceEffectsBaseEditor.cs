using UnityEngine;
using UnityEditor;
using PrecisionSurfaceEffects;

[CustomEditor(typeof(SurfaceEffectsBase), editorForChildClasses: true)]
[CanEditMultipleObjects]
public class SurfaceEffectsBaseEditor : Editor
{
    public override void OnInspectorGUI()
    {
        var seb = target as SurfaceEffectsBase;

        if(seb.soundSet.data != seb.particleSet.data)
            EditorGUILayout.HelpBox("The ParticleSet's SurfaceData doesn't match the SoundSet's", MessageType.Error);

        base.OnInspectorGUI();
    }
}