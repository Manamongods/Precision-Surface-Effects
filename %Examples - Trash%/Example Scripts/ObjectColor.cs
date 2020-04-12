#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

[RequireComponent(typeof(SurfaceBlendMarker), typeof(CollisionEffects))]
public class ObjectColor : MonoBehaviour
{
    //Fields
    public Color color = Color.white;


    //Lifecycle
    private void OnValidate()
    {
        var sbm = GetComponent<SurfaceBlendMarker>();
        for (int i = 0; i < sbm.blends.blends.Length; i++)
            sbm.blends.blends[i].color = color;

        GetComponent<CollisionEffects>().particles.selfColor = color;
    }
}
#endif