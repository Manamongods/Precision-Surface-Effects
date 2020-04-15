using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [System.Serializable]
    public class ScaledAnimationCurve
    {
        public float constant = 0;
        public float curveRange = 1000;
        public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
        public float curveMultiplier = 4;

        public float Evaluate(float t)
        {
            return constant + curveMultiplier * curve.Evaluate(t / curveRange);
        }

        public void Validate()
        {
            curve.preWrapMode = curve.postWrapMode = WrapMode.Clamp;
        }
    }
}