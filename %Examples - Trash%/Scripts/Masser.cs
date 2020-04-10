using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Masser : MonoBehaviour
{
    public KeyCode key;

    public Rigidbody rb;
    public float defaultMass = 70;
    public float bigMass = 7000;

    private void Update()
    {
        rb.mass = Input.GetKey(key) ? bigMass : defaultMass;
    }
}