/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    public class SurfaceSetType
    {
        //[ReadOnly]
        //[SeperatorLine(true)]
        [SerializeField]
        [HideInInspector]
        public string name = "";
    }

    public class SurfaceSet<T> : ScriptableObject where T : SurfaceSetType, new()
    {
        //Fields
        public SurfaceData data;
      


        //Methods
#if UNITY_EDITOR
        protected void Resize(ref T[] ts)
        {
            if (data == null)
                return;

            var l = data.surfaceTypes.Length;
            if (l > ts.Length)
                System.Array.Resize(ref ts, l);

            for (int i = 0; i < ts.Length; i++)
            {
                if (ts[i] == null)
                    ts[i] = new T();

                string s;
                if (i < data.surfaceTypes.Length)
                    s = data.surfaceTypes[i].name;
                else
                    s = "THIS DOESN'T EXIST AS A SOUND-TYPE (YET?)";
                ts[i].name = s;
            }
        }
#endif
    }
}