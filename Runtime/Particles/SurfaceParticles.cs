/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    [RequireComponent(typeof(ParticleSystem))]
    public class SurfaceParticles : MonoBehaviour
    {
        //Fields
        public SurfaceParticles[] children; //subParticleSystems

        [Space(10)]
        public bool isSelf;

        [Header("Quality")]
        [SerializeField]
        private bool inheritVelocities = true;

        [Header("Inherit Velocity")]
        [Range(0, 1)]
        public float inheritAmount = 0.5f;
        public Vector2 inheritSpreadRange = new Vector2(0, 1);

        [Header("Color")]
        public bool setColor = true; //this isn't the case for sparks or cartooney white puffs //public enum ColorType { IsSelf, IsOther, IsNone }

        [Header("Shape")]
        public float shapeRadiusScaler = 1;
        public float constantShapeRadius = 0.2f;
        public Vector3 shapeRotationOffset = new Vector3(-90, 0, 0);
        [System.NonSerialized]
        public Vector3 flipSelfRotationOffset = new Vector3(180, 0, 0);

        [Header("ColliderEffects' Speed Fading")]
        public float impactSpeedMultiplier = 1;
        [Space(5)]
        public float rollingSpeedMultiplier = 0.1f;
        public float slidingSpeedMultiplier = 1;

        [Header("Speed")]
        public float baseSpeedMultiplier = 1;
        public float speedMultiplierBySpeed = 1;

        [Header("Count")]
        public ScaledAnimationCurve rateByForce = new ScaledAnimationCurve(); //public Vector2 countBySpeedRange = new Vector2(-0.5, 2); //-1, 5
        [Min(0)]
        public float countByInverseScaleExponent = 2;

        [Header("Size")]
        public ScaledAnimationCurve scalerByForce = new ScaledAnimationCurve();

        [HideInInspector]
        public new ParticleSystem particleSystem;

        private SurfaceParticles instance;
        private ParticleSystem temporarySystem;

        private static readonly ParticleSystem.Particle[] sourceParticles = new ParticleSystem.Particle[1000];
        private static readonly ParticleSystem.Particle[] destinationParticles = new ParticleSystem.Particle[10000];

        private float startSpeedMultiplier;

        private Color c;
        private Color c0, c1;
        private ParticleSystemGradientMode colorMode;
        private ParticleSystem.MinMaxGradient sc;
        private ParticleSystem.MinMaxCurve ss, ss2;
        private ParticleSystem.MinMaxCurve ssX, ssX2, ssY, ssY2, ssZ, ssZ2;



        //Datatypes
        [System.Serializable]
        public class ScaledAnimationCurve
        {
            public float constant = 0;
            public float curveRange = 1000;
            public AnimationCurve curve = AnimationCurve.Linear(0, 0, 1, 1);
            public float curveMultiplier = 4;

            public float Evaluate(float t)
            {
                return constant + curveMultiplier * curve.Evaluate(t / curveRange);
            }

            public void Validate()
            {
                curve.preWrapMode = curve.postWrapMode = WrapMode.Clamp;
            }
        }



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

        //These are to fade your effects yourself
        public float GetAmountedSpeed(Vector3 velocity0, Vector3 contactVelocity1, Vector3 centerVelocity1)
        {
            var rollingSpeed = (velocity0 - centerVelocity1).magnitude;
            var slidingSpeed = (velocity0 - contactVelocity1).magnitude;
            return GetAmountedSpeed(rollingSpeed, slidingSpeed);
        }
        public float GetAmountedSpeed(float rollingSpeed, float slidingSpeed)
        {
            return rollingSpeed * rollingSpeedMultiplier + slidingSpeed * slidingSpeedMultiplier;
        }

        public void PlayParticles(bool flipSelf, Color selfColor, Color otherColor, float particleCountScaler, float particleSizeScaler, float weight, float impulse, float speed, Quaternion rot, Vector3 center, float radius, Vector3 normal, Vector3 vel0, Vector3 vel1, float mass0, float mass1, float dt = 0.25f, bool withChildren = true)
        {
            bool isSelf = this.isSelf ^ flipSelf;

            if (withChildren)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    var sps = children[i].GetInstance();
                    sps.PlayParticles(flipSelf, selfColor, otherColor, particleCountScaler, particleSizeScaler, weight, impulse, speed, rot, center, radius, normal, vel0, vel1, mass0, mass1, dt: dt, withChildren: false);
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
            //else
            //    Debug.Log("Empty normal");


            ParticleSystem system = inheritVelocities ? temporarySystem : particleSystem;


            radius *= shapeRadiusScaler;
            radius += constantShapeRadius;

            var main = system.main;
            main.startSpeedMultiplier = startSpeedMultiplier * (baseSpeedMultiplier + speed * speedMultiplierBySpeed);


            var shape = system.shape;
            shape.position = center;
            shape.radius = radius;
            var baseRot = Quaternion.identity;
            if (isSelf)
                baseRot = Quaternion.Euler(flipSelfRotationOffset);
            shape.rotation = (rot * Quaternion.Euler(shapeRotationOffset) * baseRot).eulerAngles;


            float force = impulse / dt;
            float scale = scalerByForce.Evaluate(force);// Mathf.Min(baseScaler + scalerByImpulse * impulse, maxScale);
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


            float countMult = particleCountScaler; // * Mathf.Clamp01(Mathf.InverseLerp(countBySpeedRange.x, countBySpeedRange.y, speed));
            countMult /= Mathf.Pow(scale, countByInverseScaleExponent); // * scale; //should technically be cubed though
            var countf = countMult * rateByForce.Evaluate(force) * dt * weight; //maxRate * dt //Mathf.Min(, maxAttemptParticleCount) 
            int count = (int)countf;
            if (Random.value < countf - count)
                count++;


            if(setColor)
            {
                var color = isSelf ? selfColor : otherColor;

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

            scalerByForce.Validate();
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
 *         //Constants
        public static int maxAttemptParticleCount = 1000; //This is to prevent excessive numbers such as from perhaps a bug



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