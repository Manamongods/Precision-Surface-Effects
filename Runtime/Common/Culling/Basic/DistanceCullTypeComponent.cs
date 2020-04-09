using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Culling
{
    public class DistanceCullTypeComponent : MonoBehaviour
    {
        //Fields
        public DistanceCullType distanceCullType;
        public float distance = 100;


        //Lifecycle
        private void Update()
        {
            distanceCullType.position = transform.position;
            distanceCullType.distance = distance;
            distanceCullType.Refresh();
        }
    }
}