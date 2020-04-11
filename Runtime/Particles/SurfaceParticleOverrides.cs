/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [CreateAssetMenu(menuName = "Precision Surface Effects/Particle Overrides")]
    public class SurfaceParticleOverrides : ScriptableObject
    {
        //Fields
        public Override[] overrides = new Override[1] { new Override() };


        //Methods
        public SurfaceParticles Get(ref SurfaceOutput output, SurfaceParticleSet particleSet)
        {
            for (int i = 0; i < overrides.Length; i++)
            {
                var o = overrides[i];
                if (o.particleSet == particleSet)
                {
                    output.particleCountMultiplier *= o.countMultiplier;
                    output.particleSizeMultiplier *= o.sizeMultiplier;
                    return o.particles;
                }
            }

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
            public SurfaceParticles particles;
            public float sizeMultiplier = 1;
            public float countMultiplier = 1;
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