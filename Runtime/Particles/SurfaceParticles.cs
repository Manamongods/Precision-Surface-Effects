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
        //Constants
        public static int maxAttemptParticleCount = 1000; //This is to prevent excessive numbers such as from perhaps a bug



        //Fields
        public SurfaceParticles[] children; //subParticleSystems

        [Header("Quality")]
        [SerializeField]
        private bool inheritVelocities = true;

        [Header("Inherit Velocity")]
        [Range(0, 1)]
        public float inheritAmount = 0.5f;
        public Vector2 inheritSpreadRange = new Vector2(0, 1);

        [Header("Color")]
        public bool setColor = true; //this isn't the case for sparks or cartooney white puffs

        [Header("Shape")]
        public float shapeRadiusScaler = 1;
        public float constantShapeRadius = 0.2f;
        public Vector3 shapeRotationOffset = new Vector3(-90, 0, 0);

        [Header("Speed")]
        public float baseSpeedMultiplier = 1;
        public float speedMultiplierBySpeed = 1;

        [Header("Count")]
        public Vector2 countBySpeedRange = new Vector2(0, 5);
        public float countByImpulse;
        [Min(0)]
        public float countByInverseScaleExponent = 2;

        [Header("Size")]
        public float baseScaler = 0.5f;
        public AnimationCurve scalerByForce = AnimationCurve.Linear(0, 0, 1, 1); //1, 1000, 5 //public float scalerByImpulse = 1; //public float baseScaler = 1;
        public float scalerForceRange = 1000;
        public float scalerByForceMultiplier = 4; //public float maxScale = 4;

        [HideInInspector]
        public new ParticleSystem particleSystem;

        private SurfaceParticles instance;
        private ParticleSystem temporarySystem;

        private static readonly ContactPoint[] contacts = new ContactPoint[64];
        private static readonly ParticleSystem.Particle[] sourceParticles = new ParticleSystem.Particle[1000];
        private static readonly ParticleSystem.Particle[] destinationParticles = new ParticleSystem.Particle[10000];

        private float startSpeedMultiplier;

        private Color c;
        private Color c0, c1;
        private ParticleSystemGradientMode colorMode;
        private ParticleSystem.MinMaxGradient sc;
        private ParticleSystem.MinMaxCurve ss, ss2;
        private ParticleSystem.MinMaxCurve ssX, ssX2, ssY, ssY2, ssZ, ssZ2;



        //Methods
        public SurfaceParticles GetInstance()
        {
            if(gameObject.scene.name != null)
                return this;

            if (instance == null)
            {
                instance = Instantiate(this);
            }

            return instance;
        }

        public static Vector3 GetVelocityMass(Rigidbody r, Vector3 point, out float mass)
        {
            if (r == null)
            {
                mass = 1E32f; // float.MaxValue; // Mathf.Infinity;
                return Vector3.zero;
            }
            else
            {
                mass = r.mass;
                return r.GetPointVelocity(point);
            }
        }
        public static void GetData(Collision c, out float impulse, out float speed, out Quaternion rot, out Vector3 center, out float radius, out Vector3 vel0, out Vector3 vel1, out float mass0, out float mass1)
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

            rot = Quaternion.FromToRotation(Vector3.forward, normal); //Vector3.up

            vel0 = GetVelocityMass(c.GetContact(0).thisCollider.attachedRigidbody, center, out mass0);
            vel1 = GetVelocityMass(c.rigidbody, center, out mass1);

            speed = (vel0 - vel1).magnitude;
        }

        public void PlayParticles(Color color, float particleCountScaler, float particleSizeScaler, float weight, float impulse, float speed, Quaternion rot, Vector3 center, float radius, Vector3 normal, Vector3 vel0, Vector3 vel1, float mass0, float mass1, float dt = 0.25f, bool withChildren = true)
        {
            if (withChildren)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    var sps = children[i].GetInstance();
                    sps.PlayParticles(color, particleCountScaler, particleSizeScaler, weight, impulse, speed, rot, center, radius, normal, vel0, vel1, mass0, mass1, dt: dt, withChildren: false);
                }
            }

            if (inheritVelocities && temporarySystem == null)
            {
                var inst = Instantiate(this);
                temporarySystem = inst.GetComponent<ParticleSystem>();
                Destroy(inst);

                temporarySystem.transform.SetParent(transform);
                temporarySystem.gameObject.name = "Temporary Buffer System";
                var em2 = temporarySystem.emission;
                em2.enabled = false;
            }

            if (normal != Vector3.zero)
            {
                float massSum = mass0 + mass1;
                Vector3 mixVel;
                if (massSum == 0)
                    mixVel = (vel0 + vel1) * 0.5f;
                else
                {
                    float t = mass1 / massSum;
                    mixVel = (1 - t) * vel0 + t * vel1;
                }

                vel0 = Vector3.Reflect(vel0 - mixVel, normal) * inheritAmount + mixVel;
                vel1 = Vector3.Reflect(vel1 - mixVel, normal) * inheritAmount + mixVel;
            }
            else
                Debug.Log("Empty normal");


            ParticleSystem system = inheritVelocities ? temporarySystem : particleSystem;


            radius *= shapeRadiusScaler;
            radius += constantShapeRadius;

            var main = system.main;
            main.startSpeedMultiplier = startSpeedMultiplier * (baseSpeedMultiplier + speed * speedMultiplierBySpeed);


            var shape = system.shape;
            shape.position = center;
            shape.radius = radius;
            shape.rotation = (rot * Quaternion.Euler(shapeRotationOffset)).eulerAngles;


            float force = impulse / dt;
            float scale = baseScaler + scalerByForceMultiplier * scalerByForce.Evaluate(force / scalerForceRange);// Mathf.Min(baseScaler + scalerByImpulse * impulse, maxScale);
            scale *= particleSizeScaler;
            
            //I have to do this bs because the startSizeMultiplier doesn't work...
            void Apply(ParticleSystem.MinMaxCurve from, ref ParticleSystem.MinMaxCurve to, float mult)
            {
                to.constant = from.constant * mult;
                to.constantMin = from.constantMin * mult;
                to.constantMax = from.constantMax * mult;
                to.curveMultiplier = from.curveMultiplier * mult;
            }
            if (main.startSize3D)
            {
                Apply(ssX, ref ssX2, scale);
                main.startSizeX = ssX2;
                Apply(ssY, ref ssY2, scale);
                main.startSizeY = ssY2;
                Apply(ssZ, ref ssZ2, scale);
                main.startSizeZ = ssZ2;
            }
            else
            {
                Apply(ss, ref ss2, scale);
                main.startSize = ss2;
            }


            float countMult = particleCountScaler * Mathf.Clamp01(Mathf.InverseLerp(countBySpeedRange.x, countBySpeedRange.y, speed));
            countMult /= Mathf.Pow(scale, countByInverseScaleExponent); // * scale; //should technically be cubed though
            var countf = Mathf.Min(countMult * countByImpulse * impulse, maxAttemptParticleCount) * weight; //maxRate * dt
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

            var par = new ParticleSystem.EmitParams();

            if (inheritVelocities)
            {
                //int dstCount = particleSystem.GetParticles(destinationParticles);

                //int maxDst = Mathf.Min(destinationParticles.Length, main.maxParticles);
                //var dstPC = particleSystem.particleCount;
                //int takingCount = Mathf.Min(maxDst - dstPC, temporarySystem.GetParticles(sourceParticles));
                int takingCount = temporarySystem.GetParticles(sourceParticles);
                for (int i = 0; i < takingCount; i++)
                {
                    var particle = sourceParticles[i];

                    float rand = Random.Range(inheritSpreadRange.x, inheritSpreadRange.y); //is there a faster version of this?
                    particle.velocity += (vel0 * (1 - rand) + vel1 * rand);

                    //destinationParticles[dstCount] = particle;
                    //dstCount++;

                    par.particle = particle;
                    particleSystem.Emit(par, 1);
                }

                //particleSystem.SetParticles(destinationParticles, dstCount);
                temporarySystem.Clear(false); // SetParticles(destinationParticles, 0);
            }

            //if(!particleSystem.isPlaying)
            //    particleSystem.Play();
        }



        //Lifecycle
#if UNITY_EDITOR
        private void OnValidate()
        {
            if (!particleSystem)
                particleSystem = GetComponent<ParticleSystem>();

            inheritSpreadRange.x = Mathf.Clamp01(inheritSpreadRange.x);
            inheritSpreadRange.y = Mathf.Clamp01(inheritSpreadRange.y);

            scalerByForce.preWrapMode = scalerByForce.postWrapMode = WrapMode.Clamp;
        }
#endif

        private void Awake()
        {
            var main = particleSystem.main;

            if (main.startSize3D)
            {
                ssX = main.startSizeX;
                ssX2 = main.startSizeX;
                ssY = main.startSizeY;
                ssY2 = main.startSizeY;
                ssZ = main.startSizeZ;
                ssZ2 = main.startSizeZ;
            }
            else
            {
                ss = main.startSize;
                ss2 = main.startSize;
            }

            startSpeedMultiplier = main.startSpeedMultiplier;

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
 *     if(main.startSize3D)
                startSizeM3D = new Vector3(main.startSizeXMultiplier, main.startSizeYMultiplier, main.startSizeZMultiplier);
            else
                startSizeM = main.startSizeMultiplier;

 *                 //var averageVel = (vel0 + vel1) * 0.5f;
                //vel0 = Vector3.Reflect(vel0 - vel1, normal) + vel1;
                //vel1 = Vector3.Reflect(vel1 - vel0, normal) + vel0;

                //vel0 = Vector3.ProjectOnPlane(vel0, normal) + averageVel;
                //vel1 = Vector3.ProjectOnPlane(vel1, normal);
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
