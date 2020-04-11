using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

public class SurfaceEffectsBase : MonoBehaviour
{
    //Fields
    public float minimumWeight = 0.1f;
    
    [Header("Sound")]
    [Space(20)]
    public SurfaceSoundSet soundSet;
    public float volumeByImpulse = 1;
    public float basePitch = 1;
    public float pitchBySpeed;

    [Header("Particles")]
    [Space(20)]
    public SurfaceParticleSet particleSet;
    public float particleRadius = 0;
    public float particleCountMultiplier = 1;
    public float particleSizeMultiplier = 1;
    public float particleDeltaTime = 0.25f; //Deltatime allows you to control how many max particles you can accept, because Time.deltaTime is irrelevant for a single shot
    [Tooltip("This is used to calculate the (reflected) inherited particle velocity")]
    public float mass = 1;

    [Space(20)]
    public LayerMask layerMask = -1;
    public float maxDistance = Mathf.Infinity;



    //Methods
    public SurfaceOutputs Play(AudioSource[] audioSources, Vector3 pos, Vector3 dir, float impulse, float speed)
    {
        var outputs = soundSet.data.GetRaycastSurfaceTypes(pos, dir, shareList: true);
        outputs.Downshift(audioSources.Length, minimumWeight);

        for (int i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];
            soundSet.PlayOneShot(output, audioSources[i], volumeByImpulse * impulse, basePitch + pitchBySpeed * speed);

            if (particleSet != null)
            {
                output.particleSizeMultiplier *= particleSizeMultiplier;
                output.particleCountMultiplier *= particleCountMultiplier;
                particleSet.PlayParticles(outputs, output, impulse, speed * dir, mass, radius: particleRadius, deltaTime: particleDeltaTime); //-speed * outputs.hitNormal
            }
        }

        return outputs;
    }
}