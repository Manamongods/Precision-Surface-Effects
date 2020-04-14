using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ObjectThrower : MonoBehaviour
{
    //Fields
    public KeyCode[] keycodes;

    public Transform axes;
    public int maxCount = 2;

    public Type[] types;
    public int id;

    public UnityEngine.UI.Text text;

    private List<GameObject> instances = new List<GameObject>();
    


    //Datatypes
    [System.Serializable]
    public class Type
    {
        public GameObject prefab;
        public float speed = 20;
        public float randomAngular = 1000;
    }



    //Lifecycle
    private void Update()
    {
        id += Mathf.RoundToInt(Input.GetAxisRaw("Mouse ScrollWheel") * 10);
        while (id < 0)
            id += types.Length;
        while (id >= types.Length)
            id -= types.Length;

        if (text)
            text.text = types[id].prefab.name;

        for (int i = 0; i < keycodes.Length; i++)
        {
            if (Input.GetKeyDown(keycodes[i]))
            {
                StartCoroutine(Coro());
                break;
            }
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



        var type = types[id];

        var g = Instantiate(type.prefab, axes);
        g.transform.SetParent(null, true);
        var rb = g.GetComponent<Rigidbody>();
        rb.velocity = axes.forward * type.speed;
        rb.angularVelocity = Random.onUnitSphere * Random.value * type.randomAngular;

        instances.Add(g);
    }
}
