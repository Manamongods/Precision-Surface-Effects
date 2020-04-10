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

            return surfaceTypeParticles[o.surfaceTypeID].particles;
        }

        public void PlayParticles(SurfaceOutputs outputs, SurfaceOutput output, float impulse, Vector3 vel, float radius = 0, float deltaTime = 0.25f)
        {
#if UNITY_EDITOR
            if (!Application.isPlaying)
                return;
#endif

            SurfaceParticles p = GetSurfaceParticles(ref output);

            if(p != null)
            {
                p = p.GetInstance();

                var rot = Quaternion.FromToRotation(Vector3.up, outputs.hitNormal);
                var otherVel = SurfaceParticles.GetVelocity(outputs.collider.attachedRigidbody, outputs.hitPosition);
                var speed = (otherVel - vel).magnitude;
                p.PlayParticles
                (
                    output.color, output.particleCountMultiplier, output.particleSizeMultiplier, 
                    1, 
                    impulse, speed, 
                    rot, outputs.hitPosition, radius, outputs.hitNormal, 
                    vel, otherVel,
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
    }
}