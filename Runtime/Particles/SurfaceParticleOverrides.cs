 /*
MIT License

Copyright (c) 2020 Steffen Vetne

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in all
copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN THE
SOFTWARE.
*/

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [CreateAssetMenu(menuName = "Precision Surface Effects/Particle Overrides")]
    public class SurfaceParticleOverrides : ScriptableObject
    {
        //Fields
        public Override[] overrides = new Override[0];


        //Methods
        public SurfaceParticles Get(SurfaceParticleSet particleSet)
        {
            for (int i = 0; i < overrides.Length; i++)
            {
                var o = overrides[i];
                if (o.particleSet == particleSet)
                    return o.particles;
            }

            return null;
        }


        //Datatypes
        [System.Serializable]
        public class Override
        {
            public SurfaceParticleSet particleSet;
            public SurfaceParticles particles;
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