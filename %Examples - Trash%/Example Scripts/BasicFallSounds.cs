using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;
using UnityEditor;

[ExecuteInEditMode]
public class BasicFallSounds : SurfaceEffectsBase
{
    //Fields
    public Transform origin;
    public float extraSearchThickness = 0.01f;
    public AudioSource[] audioSources;
    public float maxImpulse = 1000;


    //Methods
    private void OnCollisionEnter(Collision collision)
    {
        var c0 = collision.GetContact(0);
        var pos = c0.point + c0.normal * (Mathf.Abs(c0.separation) + extraSearchThickness);

        Play(audioSources, pos, -c0.normal, Mathf.Min(maxImpulse, collision.impulse.magnitude), collision.relativeVelocity.magnitude);
    }
}