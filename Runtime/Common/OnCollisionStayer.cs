using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IMayNeedOnCollisionStay
{
    bool NeedOnCollisionStay { get; }
}

[DisallowMultipleComponent]
public sealed class OnCollisionStayer : MonoBehaviour
{
    //Fields
    [HideInInspector]
    [SerializeField]
    private bool needed;

    public OnOnCollisionStay onOnCollisionStay;



    //Datatypes
    public delegate void OnOnCollisionStay(Collision collision);



    //Lifecycle
#if UNITY_EDITOR
    private void OnValidate()
    {
        if (Application.isPlaying)
            return;

        //hideFlags = HideFlags.HideInInspector;
        hideFlags = HideFlags.None;

        var inocss = GetComponents<IMayNeedOnCollisionStay>();
        if (inocss.Length == 0)
        {
            DestroyImmediate(this);
            return;
        }

        needed = false;
        foreach (var inocs in inocss)
        {
            if(inocs.NeedOnCollisionStay)
            {
                needed = true;
                break;
            }
        }
    }
#endif

    private void Start()
    {
        if (!needed)
            Destroy(this);
    }

    private void OnCollisionStay(Collision collision)
    {
        if(onOnCollisionStay != null)
            onOnCollisionStay(collision);
    }
}