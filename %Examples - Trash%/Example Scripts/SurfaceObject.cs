#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

[EditorOnly]
[RequireComponent(typeof(SurfaceBlendMarker), typeof(CollisionEffects))]
public class SurfaceObject : MonoBehaviour
{
    //Fields
    public Color color = Color.white;
    public ParticleMultipliers selfParticleMultipliers = ParticleMultipliers.Default();


    //Lifecycle
    private void OnValidate()
    {
        var sbm = GetComponent<SurfaceBlendMarker>();
        for (int i = 0; i < sbm.blends.blends.Length; i++)
        {
            var b = sbm.blends.blends[i];
            b.color = color;
            b.otherParticleMultipliers = selfParticleMultipliers; //because the other here is self
        }

        var ce = GetComponent<CollisionEffects>();
        ce.particles.selfColor = color;
        ce.particles.selfMultipliers = selfParticleMultipliers;
    }
}
#endif