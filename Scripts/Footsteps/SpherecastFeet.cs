using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

//You use PlayFootSound(int id) as an animation event

//You should scale the radius by transform.lossyScale if you want to be able to shrink the character

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
    public override void PlayFootSound(int footID, float volumeMultiplier = 1, float pitchMultiplier = 1)
    {
        volumeMultiplier *= this.volumeMultiplier;
        pitchMultiplier *= this.pitchMultiplier;

        var foot = feet[footID];


        var pos = origin.position;
        var dir = -origin.up;


        int maxCount = foot.audioSources.Length;
        var outputs = soundSet.data.GetSphereCastSurfaceTypes(pos, dir, radius: radius, maxDistance: maxDistance, layerMask: layerMask, shareList: true);
        outputs.Downshift(maxCount, minWeight);

        for (int i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];
            var vm = output.weight * output.volume * volumeMultiplier;
            var pm = output.pitch * pitchMultiplier;
            soundSet.surfaceTypeSounds[output.surfaceTypeID].PlayOneShot(foot.audioSources[i], volumeMultiplier: vm, pitchMultiplier: pm);
        }
    }


    //Lifecycle
    protected virtual void OnDrawGizmosSelected()
    {
        Gizmos.DrawWireSphere(origin.position, radius);
    }
}
