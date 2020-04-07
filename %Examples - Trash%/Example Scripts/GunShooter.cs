#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;
using UnityEditor;

[CustomEditor(typeof(GunShooter))]
public class GSEditor : Editor
{
    private void OnSceneGUI()
    {
        if (!(Event.current.type == EventType.KeyDown && Event.current.keyCode == KeyCode.R))
            return;

        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        var gs = target as GunShooter;
        gs.Shoot(ray.origin, ray.direction);
    }
}

public class GunShooter : MonoBehaviour
{
    //Fields
    public SurfaceSoundSet soundSet;
    public SurfaceParticleSet particleSet;

    public float speed = 100;
    public float impulse = 100;
    public float radius = 0;
    public float countScaler = 1;
    public float sizeScaler = 1;

    public AudioSource[] audioSources;
    public float minimumWeight = 0.1f;

    public float volumeMultiplier = 1;
    public float pitchMultiplier = 1;


    //Methods
    public void Shoot(Vector3 pos, Vector3 dir)
    {
        var outputs = soundSet.data.GetRaycastSurfaceTypes(pos, dir);
        outputs.Downshift(audioSources.Length, minimumWeight);

        for (int i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];

            soundSet.PlayOneShot(output, audioSources[i], volumeMultiplier, pitchMultiplier);

            particleSet.PlayParticles(outputs, output, impulse, speed, speed * dir, radius);
        }
    }


    //Lifecycle
    private void Update()
    {
        if(Input.GetKeyDown(KeyCode.R))
        {
            var r = Camera.main.ScreenPointToRay(Input.mousePosition);
            Shoot(r.origin, r.direction);
        }
    }
}
#endif