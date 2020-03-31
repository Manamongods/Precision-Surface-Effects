using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//You use PlayFootSound(int id) as an animation event

//This is untested though

public class RaycastAnimatorFeet : MonoBehaviour
{
    //Fields
    public SurfaceSounds surfaceSounds;
    public string soundSetName = "Player";

    [Space(20)]
    public Foot[] feet = new Foot[2];

    [Header("Raycasting")]
    public LayerMask layerMask;
    public float maxDistance = Mathf.Infinity;
    public Transform directionOverride;

    private int soundSetID;



    //Datatypes
    [System.Serializable]
    public class Foot
    {
        public AudioSource audioSource;

        [Header("Raycasting")]
        public Transform foot;
        public Vector3 raycastOffset = Vector3.zero;
        public Vector3 raycastDirection = Vector3.down;
    }



    //Methods
    public void PlayFootSound(int footID)
    {
        var foot = feet[footID];

        var pos = foot.foot.TransformPoint(foot.raycastOffset);

        Vector3 dir;
        if(directionOverride != null)
            dir = -directionOverride.up;
        else
            dir = foot.foot.TransformDirection(foot.raycastDirection);

        var st = surfaceSounds.GetRaycastSurfaceType(pos, dir, maxDistance: maxDistance, layerMask: layerMask);

        st.GetSoundSet(soundSetID).PlayOneShot(foot.audioSource);           
    }


    //Lifecycle
    private void Start()
    {
        soundSetID = surfaceSounds.FindSoundSetID(soundSetName);
    }
}
