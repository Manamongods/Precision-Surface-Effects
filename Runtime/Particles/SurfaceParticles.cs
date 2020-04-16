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
    public struct ParticleMultipliers
    {
        [Min(0)]
        public float countMultiplier;
        [Min(0)]
        public float sizeMultiplier;

        public static ParticleMultipliers Default()
        {
            return new ParticleMultipliers() { countMultiplier = 1, sizeMultiplier = 1 };
        }

        public static ParticleMultipliers operator *(ParticleMultipliers a, ParticleMultipliers b)
        {
            return new ParticleMultipliers() { countMultiplier = a.countMultiplier * b.countMultiplier, sizeMultiplier = a.sizeMultiplier * b.sizeMultiplier };
        }

        public static ParticleMultipliers operator *(ParticleMultipliers a, float b)
        {
            return new ParticleMultipliers() { countMultiplier = a.countMultiplier * b, sizeMultiplier = a.sizeMultiplier * b };
        }

        public static ParticleMultipliers operator +(ParticleMultipliers a, ParticleMultipliers b)
        {
            return new ParticleMultipliers() { countMultiplier = a.countMultiplier + b.countMultiplier, sizeMultiplier = a.sizeMultiplier + b.sizeMultiplier };
        }
    }

    [RequireComponent(typeof(ParticleSystem))]
    [AddComponentMenu("PSE/Surface Particles")]
    public class SurfaceParticles : MonoBehaviour
    {
        //Fields
        public SurfaceParticles[] children; //subParticleSystems

        [Header("Quality")]
        [SerializeField]
        private bool inheritVelocities = true; // highQuality = true;

        [Header("Inherit Velocity")]
        [Range(0, 1)]
        public float inheritAmount = 0.5f;
        public Vector2 inheritSpreadRange = new Vector2(0, 1);

        [Header("Color")]
        public bool setColor = true; //this isn't the case for sparks or cartooney white puffs //public enum ColorType { IsSelf, IsOther, IsNone }

        [Header("Shape")]
        public float shapeRadiusScaler = 1;
        public float constantShapeRadius = 0.2f;
        public Vector3 shapeRotationOffset = new Vector3(0, 0, 0);
        [System.NonSerialized]
        public Vector3 flipSelfRotationOffset = new Vector3(180, 0, 0);

        [Header("ColliderEffects' Speed Fading")]
        public float impactSpeedMultiplier = 1;
        [Space(5)]
        public float rollingSpeedMultiplier = 0.1f;
        public float slidingSpeedMultiplier = 1;

        [Header("Speed")]
        public float constantSpeedMultiplier = 0; //baseSpeedMultiplier
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

        private void AssertTemporarySystem()
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
            }
        }

        public void PlayParticles
        (
            Particles.OriginType origin,
            Color selfColor, Color otherColor,
            ParticleMultipliers selfMultipliers,
            ParticleMultipliers otherMultipliers,

            float weight,

            float impulse, float speed,

            Quaternion rot, Vector3 center, float radius, Vector3 normal,

            Vector3 vel0, Vector3 vel1,
            float mass0, float mass1,

            float dt = 0.25f,

            bool withChildren = true
        )
        {
            if (dt == 0)
                return; //?


            #region Plays Children
            if (withChildren)
            {
                for (int i = 0; i < children.Length; i++)
                {
                    var sps = children[i].GetInstance();
                    sps.PlayParticles(origin, selfColor, otherColor, selfMultipliers, otherMultipliers, weight, impulse, speed, rot, center, radius, normal, vel0, vel1, mass0, mass1, dt: dt, withChildren: false);
                }
            }
            #endregion


            AssertTemporarySystem();


            #region Reflects Velocities
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
            #endregion


            ParticleSystem system = inheritVelocities ? temporarySystem : particleSystem;
            var main = system.main;
            float force = impulse / dt;


            void Play(Color color, ParticleMultipliers particleMultipliers, bool flip)
            {
                float scale = scalerByForce.Evaluate(force) * particleMultipliers.sizeMultiplier;// Mathf.Min(baseScaler + scalerByImpulse * impulse, maxScale);
                float countMult = selfMultipliers.countMultiplier * (1f / Mathf.Pow(scale, countByInverseScaleExponent));


                #region Applies Scale
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
                #endregion


                #region Finds Count
                var countf = countMult * rateByForce.Evaluate(force) * dt * weight; //maxRate * dt //Mathf.Min(, maxAttemptParticleCount) 
                int count = (int)countf;
                if (Random.value < countf - count)
                    count++;
                if (count == 0)
                    return;
                #endregion


                //Applies Start Speed
                main.startSpeedMultiplier = startSpeedMultiplier * (constantSpeedMultiplier + speed * speedMultiplierBySpeed);


                #region Applies Shape
                var shape = system.shape;
                shape.position = center;

                shape.radius = radius * shapeRadiusScaler + constantShapeRadius;
                var baseRot = Quaternion.identity;
                if (flip)
                    baseRot = Quaternion.Euler(flipSelfRotationOffset);
                shape.rotation = (rot * Quaternion.Euler(shapeRotationOffset) * baseRot).eulerAngles;
                #endregion


                #region SetColor
                if (setColor)
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
                #endregion


                //Emits
                system.Emit(count);


                //Modifies if High Quality
                if (inheritVelocities)
                {
                    Vector2 inheritSpreadRange = this.inheritSpreadRange;
                    if(flip)
                    {
                        inheritSpreadRange.x = 1 - inheritSpreadRange.x;
                        inheritSpreadRange.y = 1 - inheritSpreadRange.y;
                    }

                    var par = new ParticleSystem.EmitParams();

                    int takingCount = temporarySystem.GetParticles(sourceParticles);
                    for (int i = 0; i < takingCount; i++)
                    {
                        var particle = sourceParticles[i];

                        float rand = Random.Range(inheritSpreadRange.x, inheritSpreadRange.y); //is there a faster version of this?
                        particle.velocity += (vel0 * (1 - rand) + vel1 * rand);
                        par.particle = particle;
                        particleSystem.Emit(par, 1);
                    }

                    temporarySystem.Clear(false);
                }
            }


            if (origin == Particles.OriginType.Both)
            {
                //This is all based on the idea that the normal is how I imagine it is
                Play(otherColor, otherMultipliers, false);
                Play(selfColor, selfMultipliers, true);
            }
            else
            {
                if (origin == Particles.OriginType.Other)
                    Play(otherColor, otherMultipliers, false);
                else
                    Play(selfColor, selfMultipliers, true);
            }
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
                // particleCountScaler; // * Mathf.Clamp01(Mathf.InverseLerp(countBySpeedRange.x, countBySpeedRange.y, speed)); //countMult ; // * scale; //should technically be cubed though
         
    // SetParticles(destinationParticles, 0);
 * 
            //if(!particleSystem.isPlaying)
            //    particleSystem.Play();

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
