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

        //public Object[] userObjects;
        //public object userData;
    }

    public class SurfaceOutputs : List<SurfaceOutput>
    {
        //Fields
        public float hardness;

        public Collider collider;
        public Vector3 hitPosition;
        public Vector3 hitNormal;

        public VibrationSound vibrationSound;

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
            //This all relies on having the outputs be sorted by decreasing weight
            
            //This (in theory) pushes the weights downward from anchor so that there is never any "popping". It should bias itself to the remaining highest weights
            //(So long as the weights given were sorted, and there aren't any outputs culled past the maxCount + 1, yet)

            for (int i = Count - 1; i >= 0; i--)
            {
                if (this[i].weight <= minWeight) //= to get rid of 0s if 0
                    RemoveAt(i);
            }

            int c = Count;
            if (c > 0)
            {
                const float epsilon = 0.0000001f;

                float anchor = this[0].weight; // + eps; //1


                float min = minWeight;
                if (c > maxCount)
                    min = Mathf.Max(min, this[maxCount].weight);
                min -= epsilon;
                float downMult = anchor / (anchor - min);

                //Clears any extras
                if (c > maxCount)
                {
                    RemoveRange(maxCount, Count - maxCount);
                }

                c = Count;
                for (int i = 0; i < c; i++)
                {
                    var o = this[i];
                    o.weight = (o.weight - anchor) * downMult + anchor;
                    this[i] = o;
                }
            }
        }

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

/*
 * 
                    //if(o.volume < 0) //???? is this possible whatsoever?
                    //{
                    //    RemoveAt(i);
                    //    i--;
                    //}
                    //else

            //int downID = maxCount;
            //if (Count > downID)

                //for (int i = downID; i >= 0; i--)
                //{
                //    if (this[i].normalizedWeight < minWeight)
                //        downID = i;
                //    else
                //        break;
                //}

 *                 var val = this[i]; //val.weight *= mult;
                this[i] = val;

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

*/
