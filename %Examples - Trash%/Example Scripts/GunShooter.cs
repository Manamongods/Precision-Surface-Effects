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

public class GunShooter : SurfaceEffectsBase
{
    //Fields
    [Space(20)]
    public AudioSource[] audioSources;
    public float speed = 100;
    public float impulse = 100;


    //Methods
    public void Shoot(Vector3 pos, Vector3 dir)
    {
        var outputs = Play(audioSources, pos, dir, impulse, speed);

        if (outputs.collider != null)
        {
            var rb = outputs.collider.attachedRigidbody;
            if (rb != null)
                rb.AddForceAtPosition(-outputs.hitNormal * impulse, outputs.hitPosition, ForceMode.Impulse); //dir 
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