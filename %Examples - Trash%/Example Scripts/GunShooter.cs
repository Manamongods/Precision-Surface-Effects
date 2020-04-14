using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;
using UnityEditor;

#if UNITY_EDITOR
[CustomEditor(typeof(GunShooter))]
public class GSEditor : Editor
{
    private void OnSceneGUI()
    {
        var gs = target as GunShooter;

        if (!(gs.keycodes.Contains(Event.current.keyCode))) //Event.current.type == EventType.KeyDown && 
            return;

        var ray = HandleUtility.GUIPointToWorldRay(Event.current.mousePosition);

        if(gs.CanShoot())
            gs.Shoot(ray.origin, ray.direction);

        Event.current.Use();
    }
}
#endif

[ExecuteInEditMode]
public class GunShooter : SurfaceEffectsBase
{
    //Fields
    public List<KeyCode> keycodes = new List<KeyCode>() { KeyCode.R, KeyCode.Mouse0 };
    [Space(20)]
    public AudioSource[] audioSources;
    public float speed = 100;
    public float impulse = 100;
    public float interval = 0.1f;
    internal float previousTime;


    //Methods
    internal bool CanShoot()
    {
        return (Time.realtimeSinceStartup - previousTime) >= interval;
    }

    public void Shoot(Vector3 pos, Vector3 dir)
    {
        previousTime = Time.realtimeSinceStartup;

        var outputs = Play(audioSources, pos, dir, impulse, speed);

        for (int i = 0; i < outputs.Count; i++)
        {
            audioSources[i].transform.position = outputs.hitPosition;
        }

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
        for (int i = 0; i < keycodes.Count; i++)
        {
            if (Input.GetKey(keycodes[i]) && CanShoot())
            {
                var r = Camera.main.ScreenPointToRay(Input.mousePosition);
                Shoot(r.origin, r.direction);
            }
        }
    }
}