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
    [RequireComponent(typeof(ParticleSystem))]
    public class SurfaceParticles : MonoBehaviour
    {
        //Fields
        [Header("Quality")]
        public bool inheritVelocities = true;

        [Header("Inherit Velocity")]
        [Range(0, 1)]
        public float inheritAmount = 1;
        public Vector2 inheritSpreadRange = new Vector2(0, 1);

        [Header("Color")]
        public bool setColor = true; //this isn't the case for sparks or cartooney white puffs

        [Header("Shape")]
        public float shapeRadiusScaler = 1;
        public float constantShapeRadius = 0.2f;

        [Header("Speed")]
        public float baseSpeedMultiplier = 1;
        public float speedMultiplierBySpeed = 1;

        [Header("Count")]
        public Vector2 countBySpeedRange = new Vector2(0, 5);
        public float countByImpulse;
        public int maxRate = 1000;

        [Header("Size")]
        public float baseScaler = 1;
        public float scalerByImpulse= 1;
        public float maxScale = 4;

        [HideInInspector]
        public new ParticleSystem particleSystem;

        private SurfaceParticles instance;
        private ParticleSystem temporarySystem;

        private static readonly ContactPoint[] contacts = new ContactPoint[64];
        private static readonly ParticleSystem.Particle[] sourceParticles = new ParticleSystem.Particle[1000];
        private static readonly ParticleSystem.Particle[] destinationParticles = new ParticleSystem.Particle[10000];

        private float startSpeedMultiplier;
        private float startSizeCM;

        private Color c;
        private Color c0, c1;
        private ParticleSystemGradientMode colorMode;
        private ParticleSystem.MinMaxGradient sc;



        //Methods
        public SurfaceParticles GetInstance()
        {
            if(gameObject.scene.name != null)
                return this;

            if (instance == null)
            {
                instance = Instantiate(this);
                instance.gameObject.hideFlags = HideFlags.DontSave;
            }

            return instance;
        }

        public static Vector3 GetVelocity(Rigidbody r, Vector3 point)
        {
            if (r == null)
                return Vector3.zero;
            return r.GetPointVelocity(point);
        }
        public static void GetData(Collision c, out float impulse, out float speed, out Quaternion rot, out Vector3 center, out float radius, out Vector3 vel0, out Vector3 vel1)
        {
            impulse = c.impulse.magnitude; //speed = c.relativeVelocity.magnitude;


            Vector3 normal;
            radius = 0;

            int contactCount = c.GetContacts(contacts);
            if (contactCount == 1)
            {
                var contact = contacts[0];
                center = contact.point;
                normal = contact.normal;
            }
            else
            {
                normal = new Vector3();
                center = new Vector3();

                for (int i = 0; i < contactCount; i++)
                {
                    var contact = contacts[i];
                    normal += contact.normal;
                    center += contact.point;
                }

                normal.Normalize();
                float invCount = 1f / contactCount;
                center *= invCount;

                for (int i = 0; i < contactCount; i++)
                {
                    var contact = contacts[i];
                    radius += (contact.point - center).magnitude; //this doesn't care if it is lateral to normal, but should it?
                }

                radius *= invCount;
            }

            rot = Quaternion.FromToRotation(Vector3.up, normal);

            vel0 = GetVelocity(c.GetContact(0).thisCollider.attachedRigidbody, center);
            vel1 = GetVelocity(c.rigidbody, center);

            speed = (vel0 - vel1).magnitude;
        }

        public void PlayParticles(Color color, float weight, float impulse, float speed, Quaternion rot, Vector3 center, float radius, Vector3 normal, Vector3 vel0, Vector3 vel1, float dt = 0.25f)
        {
            if (inheritVelocities && temporarySystem == null)
            {
                var inst = Instantiate(this);
                temporarySystem = inst.GetComponent<ParticleSystem>();
                Destroy(inst);

                temporarySystem.transform.SetParent(transform);
                temporarySystem.gameObject.name = "Temporary Buffer System";
                var em2 = temporarySystem.emission;
                em2.enabled = false;
                temporarySystem.gameObject.hideFlags = HideFlags.DontSave;

                if(normal != Vector3.zero)
                {
                    var averageVel = (vel0 + vel1) * 0.5f;
                    vel0 = Vector3.Reflect(vel0 - averageVel, normal) + averageVel;
                    vel1 = Vector3.Reflect(vel1 - averageVel, normal) + averageVel;
                }
            }

            //TODO: what is the cost of Clear()? or to just set particles, can I just use one particlesystem efficiently?

            ParticleSystem system = inheritVelocities ? temporarySystem : particleSystem;


            radius *= shapeRadiusScaler;
            radius += constantShapeRadius;

            var main = system.main;
            main.startSpeedMultiplier = startSpeedMultiplier * (baseSpeedMultiplier + speed * speedMultiplierBySpeed);


            var shape = system.shape;
            shape.position = center;
            shape.radius = radius;
            shape.rotation = rot.eulerAngles;


            float scale =  Mathf.Min(baseScaler + scalerByImpulse * impulse, maxScale);
            var ss = main.startSize;
            ss.curveMultiplier = scale * startSizeCM;
            main.startSize = ss;


            float countMult = Mathf.Clamp01(Mathf.InverseLerp(countBySpeedRange.x, countBySpeedRange.y, speed));
            var countf = Mathf.Min(countByImpulse * impulse, maxRate * dt) * weight;
            int count = (int)countf;
            if (Random.value < countf - count)
                count++;


            if(setColor)
            {
                if (colorMode == ParticleSystemGradientMode.Color)
                {
                    sc.color = this.c * color;
                }
                else if (colorMode == ParticleSystemGradientMode.TwoColors)
                {
                    sc.colorMin = c0 * color;
                    sc.colorMax = c1 * color;
                }

                main.startColor = sc;
            }

            system.Emit(count);

            if (inheritVelocities)
            {
                int dstCount = particleSystem.GetParticles(destinationParticles);

                int maxDst = Mathf.Min(destinationParticles.Length, main.maxParticles);
                var dstPC = particleSystem.particleCount;
                int takingCount = Mathf.Min(maxDst - dstPC, temporarySystem.GetParticles(sourceParticles));
                for (int i = 0; i < takingCount; i++)
                {
                    var particle = sourceParticles[i];

                    float rand = Random.Range(inheritSpreadRange.x, inheritSpreadRange.y);
                    particle.velocity += inheritAmount * (vel0 * (1 - rand) + vel1 * rand);

                    destinationParticles[dstCount] = particle;
                    dstCount++;
                }

                particleSystem.SetParticles(destinationParticles, dstCount);
                temporarySystem.Clear(); // SetParticles(destinationParticles, 0);
            }

            if(!particleSystem.isPlaying)
                particleSystem.Play();
        }



        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!particleSystem)
                particleSystem = GetComponent<ParticleSystem>();

            inheritSpreadRange.x = Mathf.Clamp01(inheritSpreadRange.x);
            inheritSpreadRange.y = Mathf.Clamp01(inheritSpreadRange.y);
        }
#endif

        private void Start()
        {
            startSizeCM = particleSystem.main.startSize.curveMultiplier;
            startSpeedMultiplier = particleSystem.main.startSpeedMultiplier;

            transform.SetParent(null);
            transform.position = Vector3.zero;
            transform.rotation = Quaternion.identity;
            transform.localScale = Vector3.one;

            var e = particleSystem.emission;
            e.enabled = false;

            //Start Color
            var m = particleSystem.main;
            sc = m.startColor;
            colorMode = sc.mode;
            if (colorMode == ParticleSystemGradientMode.Color)
            {
                c = sc.color;
            }
            else if (colorMode == ParticleSystemGradientMode.TwoColors)
            {
                c0 = sc.colorMin;
                c1 = sc.colorMax;
            }
            //add others?
        }
    }
}

/*
 * #if UNITY_EDITOR
                if (!Application.isPlaying)
                    DestroyImmediate(inst);
                else
#endif
*/