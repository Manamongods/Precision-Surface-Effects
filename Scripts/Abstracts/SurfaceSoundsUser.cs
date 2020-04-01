using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public abstract class SurfaceSoundsUser : MonoBehaviour
{
    //Fields
    public SurfaceSounds surfaceSounds;
    public string soundSetName = "Player";
    public int soundSetID = -1;


    //Lifecycle
    protected virtual void OnValidate()
    {
        try
        {
            soundSetID = surfaceSounds.FindSoundSetID(soundSetName);
        }
        catch { }
    }
}
