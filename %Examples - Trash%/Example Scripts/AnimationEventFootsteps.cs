using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class AnimatorEventFootsteps : MonoBehaviour
{
    //Fields
    public CastFeet feet;

    public Animator animator;
    public string speedName = "Forward";

    public float impulseBySpeed = 10;
    public float speedBySpeed = 10;


    //Methods
    public void PlayFootSound(int footID)
    {
        float speed = animator.GetFloat(speedName);

        feet.PlayFootSound(footID, impulse: impulseBySpeed * speed, speed: speedBySpeed * speed);
    }
}