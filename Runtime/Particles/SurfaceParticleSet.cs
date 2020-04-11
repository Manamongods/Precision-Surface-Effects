/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityExtensions;

namespace PrecisionSurfaceEffects
{
    [CreateAssetMenu(menuName = "Precision Surface Effects/Surface Sound Set")]
    public class SurfaceParticleSet : SurfaceSet<SurfaceTypeParticles>
    {
        //Fields
        [Space(30)]
        [ReorderableList()]
        public SurfaceTypeParticles[] surfaceTypeParticles = new SurfaceTypeParticles[] { new SurfaceTypeParticles() };


        //Methods
        public SurfaceParticles GetSurfaceParticles(ref SurfaceOutput o)
        {
            if (o.particleOverrides != null)
            {
                var sp = o.particleOverrides.Get(ref o, this);
                if (sp != null)
                    return sp;
            }

            var stp = surfaceTypeParticles[o.surfaceTypeID];
            o.particleCountMultiplier *= stp.countMultiplier;
            o.particleSizeMultiplier *= stp.sizeMultiplier;
            return stp.particles;
        }

        public void PlayParticles(SurfaceOutputs outputs, SurfaceOutput output, float impulse, Vector3 vel, float mass, float radius = 0, float deltaTime = 0.25f)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            SurfaceParticles p = GetSurfaceParticles(ref output);

            if(p != null)
            {
                p = p.GetInstance();

                var rot = Quaternion.FromToRotation(Vector3.forward, outputs.hitNormal);
                var otherVel = Utility.GetVelocityMass(outputs.collider.attachedRigidbody, outputs.hitPosition, out float mass1);
                var speed = (otherVel - vel).magnitude;
                p.PlayParticles
                (
                    output.color, output.particleCountMultiplier, output.particleSizeMultiplier, 
                    1, 
                    impulse, speed,
                    rot, outputs.hitPosition, radius, outputs.hitNormal,
                    vel, otherVel,
                    mass, mass1,
                    dt: deltaTime
                );
            }
        }


        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            Resize(ref surfaceTypeParticles);
        }
#endif

        private void OnEnable()
        {
            //instantiates
        }
    }

    [System.Serializable]
    public class SurfaceTypeParticles : SurfaceSetType
    {
        public SurfaceParticles particles;

        public float countMultiplier = 1;
        public float sizeMultiplier = 1;
    }
}