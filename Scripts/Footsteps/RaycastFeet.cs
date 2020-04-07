using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

//You use PlayFootSound(int id) as an animation event

public class RaycastFeet : CastFeet
{
    //Fields
    [Space(20)]
    public Foot[] feet = new Foot[2];
    [Space(20)]
    [Tooltip("This is optional")]
    public Transform directionOverride;


    //Datatypes
    [System.Serializable]
    public class Foot
    {
        public AudioSource[] audioSources;

        [Header("Raycasting")]
        public Transform foot;
        public Vector3 raycastOffset = Vector3.zero;
        [Tooltip("Unless it's overrided")]
        public Vector3 raycastDirection = Vector3.down;
    }


    //Methods
    public override void PlayFootSound(int footID, float impulse, float speed)
    {
        var foot = feet[footID];

        var pos = foot.foot.TransformPoint(foot.raycastOffset);

        Vector3 dir;
        if(directionOverride != null)
            dir = -directionOverride.up;
        else
            dir = foot.foot.TransformDirection(foot.raycastDirection);

        Play(foot.audioSources, pos, dir, impulse, speed);
    }
}
