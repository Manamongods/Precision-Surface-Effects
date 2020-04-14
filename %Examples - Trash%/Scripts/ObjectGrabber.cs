using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectGrabber : MonoBehaviour
{
    //Fields
    public Camera camera;

    public KeyCode[] keycodes;
    public float acceleration = 20;

    private Collider c;
    private Rigidbody rb;
    private Vector3 colliderPoint;
    private Vector3 cameraPoint;



    //Lifecycle
    private void Update()
    {
        bool any = false;
        for (int i = 0; i < keycodes.Length; i++)
        {
            if (Input.GetKey(keycodes[i]))
            {
                var camera = this.camera;
                if(camera == null)
                    camera = Camera.main;

                if (c == null)
                {
                    if(Physics.Raycast(camera.ScreenPointToRay(Input.mousePosition), out RaycastHit rh))
                    {
                        c = rh.collider;
                        rb = c.attachedRigidbody;
                        colliderPoint = c.transform.InverseTransformPoint(rh.point);
                        cameraPoint = camera.transform.InverseTransformPoint(rh.point);
                    }
                }

                if(rb != null)
                {
                    var from = c.transform.TransformPoint(colliderPoint);
                    var to = camera.transform.TransformPoint(cameraPoint);
                    rb.AddForceAtPosition((to - from) * acceleration, from, ForceMode.Acceleration);
                }

                any = true;
                break;
            }
        }

        if(!any)
            c = null;
    }
}
