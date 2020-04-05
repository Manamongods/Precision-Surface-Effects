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
    public override void PlayFootSound(int footID, float volumeMultiplier = 1, float pitchMultiplier = 1)
    {
        volumeMultiplier *= this.volumeMultiplier;
        pitchMultiplier *= this.pitchMultiplier;

        var foot = feet[footID];

        var pos = foot.foot.TransformPoint(foot.raycastOffset);

        Vector3 dir;
        if(directionOverride != null)
            dir = -directionOverride.up;
        else
            dir = foot.foot.TransformDirection(foot.raycastDirection);


        int maxCount = foot.audioSources.Length;
        var outputs =  soundSet.data.GetRaycastSurfaceTypes
        (
            pos, dir, 
            maxOutputCount: maxCount, shareList: true, //shareList is used to avoid reallocations, but it means you have to use the outputs' information immediately
            maxDistance: maxDistance, layerMask: layerMask
        );
        outputs.Downshift(maxCount, minWeight); //This is used to smoothly cull. Until you do this, it is not guaranteed to be fewer outputs than (maxOutputs + 1)

        for (int i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];
            var vm = output.weight * output.volume * volumeMultiplier;
            var pm = output.pitch * pitchMultiplier;
            soundSet.surfaceTypeSounds[output.surfaceTypeID].PlayOneShot(foot.audioSources[i], volumeMultiplier: vm, pitchMultiplier: pm);
        }
    }
}
