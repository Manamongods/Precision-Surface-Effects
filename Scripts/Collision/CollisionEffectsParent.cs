using System.Collections;
using System.Collections.Generic;
using UnityEngine;

//This is used when you want to attach a CollisionSounds to a child collider of a rigidbody.
//It won't receive any collision callbacks, so this is the way to get different CollisionSounds variants among the colliders.

namespace PrecisionSurfaceEffects
{
    [DisallowMultipleComponent]
    public class CollisionEffectsMaker : MonoBehaviour
    {
        [Tooltip("If bigger than a colliding CollisionSounds, it will play instead of it")]
        public int priority;
    }

    public class CollisionEffectsParent : CollisionEffectsMaker
    {
        //Fields
        public Type[] types;


        //Datatypes
        [System.Serializable]
        public class Type
        {
            public Collider[] colliders;
            public CollisionSounds collisionSounds;
        }


        //Lifecycle
        private void OnCollisionEnter(Collision collision)
        {
            var thisCollider = collision.GetContact(0).thisCollider;

            for (int i = 0; i < types.Length; i++)
            {
                var t = types[i];

                for (int ii = 0; ii < t.colliders.Length; ii++)
                {
                    if (t.colliders[ii] == thisCollider)
                    {
                        t.collisionSounds.OnCollisionEnter(collision);

                        return;
                    }
                }
            }
        }

        private void OnCollisionStay(Collision collision)
        {
            var thisCollider = collision.GetContact(0).thisCollider;

            for (int i = 0; i < types.Length; i++)
            {
                var t = types[i];

                for (int ii = 0; ii < t.colliders.Length; ii++)
                {
                    if (t.colliders[ii] == thisCollider)
                    {
                        t.collisionSounds.OnCollisionStay(collision);

                        return;
                    }
                }
            }
        }
    }
}