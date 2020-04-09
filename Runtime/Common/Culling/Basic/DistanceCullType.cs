using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Culling
{
    [CreateAssetMenu(menuName = "Culling/Distance Cull Type")]
    public class DistanceCullType : CullType
    {
        //Fields
        public Vector3 position;
        public float distance;
        private float sqrDistance;


        //Methods
        public override bool EnabledAt(Cullable c, Vector3 position)
        {
            return (position - this.position).sqrMagnitude < sqrDistance * c.importance;
        }

        public void Refresh()
        {
            sqrDistance = distance * distance;
        }


        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            Refresh();
        }
#endif
    }
}