using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

public abstract class CastFeet : MonoBehaviour
{
    //Fields
    public SurfaceSoundSet soundSet;
    public float minWeight = 0.2f;

    [Space(20)]
    public float volumeMultiplier = 1;
    public float pitchMultiplier = 1;

    [Space(20)]
    public LayerMask layerMask = -1;
    public float maxDistance = Mathf.Infinity;



    //Methods
    public abstract void PlayFootSound(int footID, float volumeMultiplier = 1, float pitchMultiplier = 1);
}
