/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [System.Serializable]
    public class VibrationContribution //seperate rolling and sliding?
    {
        [Min(0)]
        public float impactForceMultiplier = 1;
        public float impactSpeedMultiplier = -1;
        [Space(5)]
        [Min(0)]
        public float frictionForceMultiplier = 1;
        [Min(0)]
        public float frictionSpeedMultiplier = 1;
    }

    public abstract class VibrationSound : MonoBehaviour 
    {
        //Fields
        public SoundScaling scaling = SoundScaling.Default();

        [Space(10)]
        public VibrationContribution contribution = new VibrationContribution();



        //Methods
        public void AddImpact(Vector3 position, float impulse, float speed) //Add direction as well?
        {
            impulse *= scaling.forceMultiplier;
            speed *= scaling.speedMultiplier;
            AddVibration(position, impulse * contribution.impactForceMultiplier, speed * contribution.impactSpeedMultiplier);
        }
        public void AddFriction(Vector3 position, float impulse, float speed)
        {
            impulse *= scaling.forceMultiplier;
            speed *= scaling.speedMultiplier;
            AddVibration(position, impulse * contribution.frictionForceMultiplier, speed * contribution.frictionSpeedMultiplier);
        }

        protected abstract void AddVibration(Vector3 position, float impulse, float speed);

        public abstract void ResetSounds();
    }
}