using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This script you should customize heavily so that it works with your own setup. It is not meant to be this way.

public class SimpleFootsteps : MonoBehaviour
{
    //Fields
    public CastFeet feet;

    public Animator animator;
    public int layerIndex;
    public string stateName = "Grounded";

    public string speedName = "Forward";
    public float basePitch = 1;
    public float pitchBySpeed;

    public Time[] times = new Time[2]
    {
        new Time() { time = 0.25f, id = 0 },
        new Time() { time = 0.75f, id = 1 }
    };

    private float previousNormalizedTime;


    //Datatypes
    [System.Serializable]
    public class Time
    {
        public float time;
        public int id;
    }


    //Methods
    private static bool Passed(float prevNT, float currNT, float t)
    {
        if (prevNT > currNT)
        {
            if (t >= prevNT)
                currNT++;
            else if (t < currNT)
                prevNT--;
        }

        if (prevNT <= t && t < currNT)
            return true;

        return false;
    }


    //Lifecycle
    private void Update()
    {
        var state = animator.GetCurrentAnimatorStateInfo(layerIndex);
        if (state.IsName(stateName))
        {
            var normalizedTime = state.normalizedTime % 1;
            float speed = animator.GetFloat(speedName);

            for (int i = 0; i < times.Length; i++)
            {
                var t = times[i];
                if (Passed(previousNormalizedTime, normalizedTime, t.time))
                    feet.PlayFootSound(t.id, speed, basePitch + speed * pitchBySpeed);
            }

            previousNormalizedTime = normalizedTime;
        }
    }
}