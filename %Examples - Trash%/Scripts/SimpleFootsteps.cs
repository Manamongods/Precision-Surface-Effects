using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script you should customize heavily so that it works with your own setup. It is not meant to be this way.

public class SimpleFootsteps : MonoBehaviour
{
    //Fields
    public RaycastAnimatorFeet feet;

    public Animator animator;
    public int layerIndex;
    public string stateName = "Grounded";

    public string speedName = "Forward";
    public float basePitch = 0.5f;
    public float pitchBySpeed;

    public Time[] times;

    private float previousNT;


    //Datatypes
    [System.Serializable]
    public class Time
    {
        public float time;
        public int id;
    }


    //Methods
    private static bool Passed(float prev, float curr, float t)
    {
        if (prev > curr)
        {
            if (t >= prev)
                curr++;
            else if (t < curr)
                prev--;
        }

        if (prev <= t && t < curr)
            return true;

        return false;
    }


    //Lifecycle
    private void Update()
    {
        var state = animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (state.IsName(stateName))
        {
            var nt = state.normalizedTime % 1;
            float amount = animator.GetFloat(speedName);

            for (int i = 0; i < times.Length; i++)
            {
                var t = times[i];
                if (Passed(previousNT, nt, t.time))
                    feet.PlayFootSound(t.id, amount, basePitch + amount * pitchBySpeed);
            }

            previousNT = nt;
        }
    }
}