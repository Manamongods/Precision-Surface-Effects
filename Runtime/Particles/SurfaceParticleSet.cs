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
        public SurfaceParticles GetSurfaceParticles(ref SurfaceOutput o, out bool flipSelf, out bool isBoth)
        {
            if (o.particleOverrides != null)
            {
                var sp = o.particleOverrides.Get(ref o, this, out flipSelf, out isBoth);
                if (sp != null)
                {
                    return sp;
                }
            }

            var stp = surfaceTypeParticles[o.surfaceTypeID];
            o.selfParticleMultipliers *= stp.selfMultipliers;
            o.otherParticleMultipliers *= stp.otherMultipliers;
            flipSelf = stp.flipSelf;
            isBoth = stp.isBoth;
            return stp.particles;
        }

        public void PlayParticles(SurfaceOutputs outputs, SurfaceOutput output, Color selfColor, float impulse, Vector3 vel, float mass, float radius = 0, float deltaTime = 0.25f)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            SurfaceParticles p = GetSurfaceParticles(ref output, out bool flipSelf, out bool isBoth);

            if(p != null)
            {
                var rot = Quaternion.FromToRotation(Vector3.forward, outputs.hitNormal);
                var otherVel = Utility.GetVelocityMass(outputs.collider.attachedRigidbody, outputs.hitPosition, out Vector3 centerVel1, out float mass1);
                var speed = (otherVel - vel).magnitude;
                p.GetInstance().PlayParticles
                (
                    flipSelf, isBoth,
                    selfColor, output.color,
                    output.selfParticleMultipliers, output.otherParticleMultipliers, 
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

        public ParticleMultipliers selfMultipliers = ParticleMultipliers.Default();
        public ParticleMultipliers otherMultipliers = ParticleMultipliers.Default();
        public bool flipSelf;
        public bool isBoth;
    }
}