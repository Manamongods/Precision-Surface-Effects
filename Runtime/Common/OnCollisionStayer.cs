using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IOnOnCollisionStay
{
    void OnOnCollisionStay(Collision collision);
}

[DisallowMultipleComponent]
public sealed class OnCollisionStayer : MonoBehaviour
{
    //Fields
    private OnOnCollisionStay onOnCollisionStay;


    //Methods
    public static void Add(GameObject gameObject, IOnOnCollisionStay self)
    {
        var ocs = gameObject.GetComponent<OnCollisionStayer>();
        if (ocs == null)
            ocs = gameObject.AddComponent<OnCollisionStayer>();

        ocs.onOnCollisionStay += self.OnOnCollisionStay;
    }
    public static void Remove(GameObject gameObject, IOnOnCollisionStay self)
    {
        var ocs = gameObject.GetComponent<OnCollisionStayer>();
        if (ocs != null)
        {
            ocs.onOnCollisionStay -= self.OnOnCollisionStay;
            if (ocs.onOnCollisionStay == null)
                Destroy(ocs);
        }
    }


    //Datatypes
    private delegate void OnOnCollisionStay(Collision collision);


    //Lifecycle
    private void OnCollisionStay(Collision collision)
    {
        onOnCollisionStay(collision);
    }
}