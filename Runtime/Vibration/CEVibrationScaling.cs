#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [EditorOnly]
    [RequireComponent(typeof(VibrationSound))]
    [AddComponentMenu("PSE/Helpers/Collision-Effects Vibration Scaling")]
    public class CEVibrationScaling : MonoBehaviour
    {
        public CollisionEffects collisionEffects;

        private void OnValidate()
        {
            if (!collisionEffects)
                collisionEffects = GetComponentInParent<CollisionEffects>();

            if(collisionEffects)
            {
                var vs = GetComponent<VibrationSound>();
                vs.scaling = collisionEffects.soundScaling;
            }
        }
    }
}
#endif