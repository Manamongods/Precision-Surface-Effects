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
    public class Particles
    {
        public SurfaceParticles particles;
        public ParticleMultipliers selfMultipliers = ParticleMultipliers.Default();
        public ParticleMultipliers otherMultipliers = ParticleMultipliers.Default();

        public OriginType originType = OriginType.Other;
        public enum OriginType { Other, Self, Both }
    }

    [CreateAssetMenu(menuName = "Precision Surface Effects/Particle Overrides")]
    public class SurfaceParticleOverrides : ScriptableObject
    {
        //Fields
        public Override[] overrides = new Override[1] { new Override() };


        //Methods
        public Particles[] Get(ref SurfaceOutput output, SurfaceParticleSet particleSet) //, out bool flipSelf, out bool isBoth
        {
            for (int i = 0; i < overrides.Length; i++)
            {
                var o = overrides[i];
                if (o.particleSet == particleSet)
                {
                    //output.selfParticleMultipliers *= o.selfMultipliers; // // //Note that these are flipped!:
                    //output.otherParticleMultipliers *= o.otherMultipliers;
                    //flipSelf = o.flipSelf;
                    //isBoth = o.isBoth;
                    return o.particles;
                }
            }

            //flipSelf = false;
            //isBoth = false;
            return null;
        }


        //Datatypes
        [System.Serializable]
        public class Override
        {
            [HideInInspector]
            [SerializeField]
            internal string autoHeader;
            public SurfaceParticleSet particleSet;

            //public SurfaceParticles particles;
            public ParticleMultipliers selfMultipliers = ParticleMultipliers.Default();
            public ParticleMultipliers otherMultipliers = ParticleMultipliers.Default();
            public bool flipSelf;
            public bool isBoth;

            public Particles[] particles = new Particles[1] { new Particles() };
        }


        //Lifecycle
        private void OnValidate()
        {
            for (int i = 0; i < overrides.Length; i++)
            {
                var o = overrides[i];
                if (o.particleSet != null)
                    o.autoHeader = o.particleSet.name;
                else
                    o.autoHeader = "";
            }
        }
    }
}

/*
 *
            // startSize.curveMultiplier;
 
    
        private static readonly List<SurfaceParticles> checks = new List<SurfaceParticles>();
 *             checks.Add(this);
            while(checks.Count > 0)
            {
                for (int i = 0; i < subParticleSystems.Length; i++)
                {
                    var sps = subParticleSystems[i];

                    if(sps == )

                    checks.Add(sps);
                }
            }
            

 * #if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(inst);
                else
#endif
*/