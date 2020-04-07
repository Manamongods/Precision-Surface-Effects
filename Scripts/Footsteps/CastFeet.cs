using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

public abstract class CastFeet : SurfaceEffectsBase
{
    //Methods
    public abstract void PlayFootSound(int footID, float impulse = 1, float speed = 1); //0, 0?
}
