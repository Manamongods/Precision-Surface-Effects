using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Graviter : MonoBehaviour
{
    public KeyCode key;

    public Rigidbody rb;
    public float bigGravity = 10;

    private void FixedUpdate()
    {
        if (Input.GetKey(key))
            rb.AddForce(Vector3.down * bigGravity, ForceMode.Acceleration);
    }
}