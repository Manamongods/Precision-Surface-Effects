using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SurfaceSoundsUser : MonoBehaviour
{
    //Fields
    public SurfaceSounds surfaceSounds;
    public string soundSetName = "Player";

    protected int soundSetID;


    //Lifecycle
    protected virtual void Start()
    {
        if (soundSetName != "")
            soundSetID = surfaceSounds.FindSoundSetID(soundSetName);
    }
}
