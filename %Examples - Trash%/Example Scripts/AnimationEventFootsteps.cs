using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEventFootsteps : MonoBehaviour
{
    //Fields
    public CastFeet feet;

    public Animator animator;
    public string speedName = "Forward";
    public float basePitch = 1;
    public float pitchBySpeed;


    //Methods
    public void PlayFootSound(int footID)
    {
        float speed = animator.GetFloat(speedName);

        feet.PlayFootSound(footID, speed, basePitch + speed * pitchBySpeed);
    }
}