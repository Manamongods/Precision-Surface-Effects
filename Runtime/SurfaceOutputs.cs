/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    public struct SurfaceOutput
    {
        public int surfaceTypeID;
        public float weight;

        public SurfaceParticleOverrides particleOverrides;
        public Color color;

        public float volumeMultiplier;
        public float pitchMultiplier;

        public ParticleMultipliers selfParticleMultipliers;
        public ParticleMultipliers otherParticleMultipliers;
    }

    public class SurfaceOutputs : List<SurfaceOutput>
    {
        //Fields
        public float hardness;

        public Collider collider;
        public Vector3 hitPosition;
        public Vector3 hitNormal;

        private static readonly SOSorter soSorter = new SOSorter();


        //Methods
        public void CopyTo(SurfaceOutputs to)
        {
            to.Clear();
            to.AddRange(this);

            to.hardness = hardness;
            to.collider = collider;
            to.hitPosition = hitPosition;
            to.hitNormal = hitNormal;
        }

        public void Downshift(int maxCount = 1, float minWeight = 0) //, float mult = 1)
        {
            //This (in theory) pushes the weights downward from anchor so that there is never any "popping". It should bias itself to the remaining highest weights
            //(So long as the weights given were sorted, and there aren't any outputs culled past the maxCount + 1, yet)

            //This all relies on having the outputs be sorted by decreasing weight
            int c = Count;
            for (int i = 0; i < c; i++)
            {
                var val = this[i]; //val.weight *= mult;
                this[i] = val;

                if (this[i].weight <= minWeight) //= to get rid of 0s if 0
                {
                    RemoveAt(i);
                    i--;
                    c--;
                }
            }

            //int downID = maxCount;
            //if (Count > downID)
            if (c > 0)
            {
                const float eps = 0.0000001f;

                //for (int i = downID; i >= 0; i--)
                //{
                //    if (this[i].normalizedWeight < minWeight)
                //        downID = i;
                //    else
                //        break;
                //}

                float anchor = this[0].weight; // + eps; //1


                float min = minWeight;
                if (c > maxCount)
                    min = Mathf.Max(min, this[maxCount].weight);
                min -= eps;
                float downMult = anchor / (anchor - min);

                //Clears any extras
                while (c > maxCount)
                {
                    RemoveAt(Count - 1);
                    c--;
                }


                for (int i = 0; i < c; i++)
                {
                    var o = this[i];
                    o.weight = (o.weight - anchor) * downMult + anchor;

                    //if(o.volume < 0) //???? is this possible whatsoever?
                    //{
                    //    RemoveAt(i);
                    //    i--;
                    //}
                    //else
                    this[i] = o;
                }
            }
        }

        //public void CombineSounds()
        //{
        //    for (int i = 0; i < Count; i++)
        //    {
        //        var so = this[i];

        //        for (int ii = i + 1; ii < Count; ii++)
        //        {
        //            var so2 = this[ii];
        //            if (so.surfaceTypeID == so2.surfaceTypeID)
        //            {
        //                float sum = so.;

        //                RemoveAt(ii);
        //                ii--;
        //            }
        //        }
        //    }
        //}

        internal void SortDescending()
        {
            Sort(soSorter);
        }


        //Constructors
        public SurfaceOutputs(SurfaceOutputs so) : base(so) { }
        public SurfaceOutputs() : base() { }


        //Datatypes
        private class SOSorter : IComparer<SurfaceOutput>
        {
            public int Compare(SurfaceOutput x, SurfaceOutput y)
            {
                return y.weight.CompareTo(x.weight); //Descending
            }
        }
    }
}