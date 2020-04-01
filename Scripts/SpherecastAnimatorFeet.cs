using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//You use PlayFootSound(int id) as an animation event

//This is untested though

public class SphereCastAnimatorFeet : MonoBehaviour
{
    //Fields
    public SurfaceSoundSet soundSet;

    [Space(20)]
    public Foot[] feet = new Foot[2];

    [Header("Sphere Cast")]
    public LayerMask layerMask = -1;
    public float maxDistance = Mathf.Infinity;
    public Transform origin;
    public float radius = 1;


    //Datatypes
    [System.Serializable]
    public class Foot
    {
        public AudioSource audioSource;
    }


    //Methods
    public void PlayFootSound(int footID)
    {
        var foot = feet[footID];

        var pos = origin.position;
        var dir = -origin.up;
        int sID = soundSet.surfaceTypes.GetSphereCastSurfaceTypeID(pos, dir, radius: radius, maxDistance: maxDistance, layerMask: layerMask);
        soundSet.surfaceTypeSounds[sID].PlayOneShot(foot.audioSource);           
    }
}
