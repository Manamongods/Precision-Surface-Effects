using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BallShooter : MonoBehaviour
{
    public GameObject prefab;
    public float speed;
    public Transform axes;
    public int maxCount = 2;

    private List<GameObject> instances = new List<GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.E))
        {
            if(instances.Count >= maxCount)
            {
                Destroy(instances[0]);
                instances.RemoveAt(0);
            }

            var g = Instantiate(prefab, axes);
            g.transform.SetParent(null, true);
            g.GetComponent<Rigidbody>().velocity = axes.forward * speed;

            instances.Add(g);
        }
    }
}
