using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Culling
{
    public class Culler : MonoBehaviour
    {
        //Fields
        [Min(0)]
        public float interval = 1;
        public CullType[] cullTypes;

        private int id;


        //Methods
        private IEnumerator Coro()
        {
            var wfs = new WaitForSeconds(interval);

            while (true)
            {
                for (int i = 0; i < cullTypes.Length; i++)
                {
                    cullTypes[i].Cull();
                }

                yield return wfs;
            }
        }


        //Lifecycle
        private void OnEnable()
        {
            StartCoroutine(Coro());
        }
    }
}