/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Malee;

namespace PrecisionSurfaceEffects
{
    [CreateAssetMenu(menuName = "Precision Surface Effects/Surface Sound Set")]
    public class SurfaceParticleSet : SurfaceSet<SurfaceTypeParticles>
    {
        //Fields
        //[Space(30)]
        //[Reorderable()] //surrogateType = typeof(SurfaceTypeParticles), surrogateProperty = "objectProperty")]
        public SurfaceTypeParticles[] surfaceTypeParticles = new SurfaceTypeParticles[] { new SurfaceTypeParticles() };

        [System.Serializable]
        public class STPArray : ReorderableArray<SurfaceTypeParticles> { }


        //Methods
        public Particles[] GetSurfaceParticles(SurfaceOutput o)
        {
            if (o.particleOverrides != null)
            {
                var sps = o.particleOverrides.Get(ref o, this);
                if (sps != null)
                {
                    return sps;
                }
            }

            var stp = surfaceTypeParticles[o.surfaceTypeID];
            return stp.particles;
        }

        public void PlayParticles(SurfaceOutputs outputs, SurfaceOutput output, Color selfColor, float impulse, Vector3 vel, float mass, float radius = 0, float deltaTime = 0.25f)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            Particles[] ps = GetSurfaceParticles(output);

            for (int i = 0; i < ps.Length; i++)
            {
                var p = ps[i];

                var rot = Quaternion.FromToRotation(Vector3.forward, outputs.hitNormal);
                var otherVel = Utility.GetVelocityMass(outputs.collider.attachedRigidbody, outputs.hitPosition, out Vector3 centerVel1, out float mass1);
                var speed = (otherVel - vel).magnitude;
                p.particles.GetInstance().PlayParticles
                (
                    p.originType,
                    selfColor, output.color,
                    output.selfParticleMultipliers * p.selfMultipliers, output.otherParticleMultipliers * p.otherMultipliers, 
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
            //Resize(ref surfaceTypeParticles);
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
        public Particles[] particles = new Particles[1] { new Particles() };
    }
}