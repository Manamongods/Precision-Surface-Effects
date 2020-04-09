using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectShooter : MonoBehaviour
{
    public KeyCode key;

    public GameObject prefab;
    public float speed;
    public Transform axes;
    public int maxCount = 2;
    public float randomAngular = 100;

    private List<GameObject> instances = new List<GameObject>();

    private void Update()
    {
        if (Input.GetKeyDown(key))
        {
            StartCoroutine(Coro());
        }
    }
    private IEnumerator Coro()
    {
        if (instances.Count >= maxCount)
        {
            instances[0].gameObject.SetActive(false);
            yield return null;
            yield return null;
            Destroy(instances[0]);
            instances.RemoveAt(0);
        }

        var g = Instantiate(prefab, axes);
        g.transform.SetParent(null, true);
        var rb = g.GetComponent<Rigidbody>();
        rb.velocity = axes.forward * speed;
        rb.angularVelocity = Random.onUnitSphere * Random.value * randomAngular;

        instances.Add(g);
    }
}
