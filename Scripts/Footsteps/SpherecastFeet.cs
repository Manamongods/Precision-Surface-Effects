using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

//You should scale the radius by transform.lossyScale if you want to be able to shrink the character

[AddComponentMenu("PSE/Footsteps/SphereCast Feet", 1000)]
public class SphereCastFeet : CastFeet
{
    //Fields
    [Space(20)]
    public Foot[] feet = new Foot[2];

    [Space(20)]
    public Transform origin;
    public float radius = 1;


    //Datatypes
    [System.Serializable]
    public class Foot
    {
        public AudioSource[] audioSources;
    }


    //Methods
    public override void PlayFootSound(int footID, float impulse, float speed)
    {
        var foot = feet[footID];

        var pos = origin.position;
        var dir = -origin.up;

        Play(foot.audioSources, pos, dir, impulse, speed);
    }


    //Lifecycle
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(origin.position, radius);
    }
}
