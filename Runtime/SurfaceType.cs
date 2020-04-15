/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

//This contains the identification of a surface type

namespace PrecisionSurfaceEffects
{
    [System.Serializable]
    public class STSettings
    {
        public float hardness = 1;
        public Color defaultColor = Color.white;
        public float volumeMultiplier = 1;
        public float pitchMultiplier = 1;

        [Header("Custom User Data")]
        public Object[] userObjects = new Object[0];
        [System.NonSerialized]
        public object userData;
    }

    [CreateAssetMenu(menuName = "Precision Surface Effects/Surface Type")]
    public class SurfaceType : ScriptableObject
    {
        public float hardnessMultiplier = 1;
        public Color defaultColorTint = Color.white;

        [Space(30)]
        public SubType[] subTypes = new SubType[1] { new SubType() };

        [Header("Custom User Data")] 
        public Object[] defaultUserObjects = new Object[0];
        [System.NonSerialized]
        public object defaultUserData;

        [System.Serializable]
        public class SubType
        {
            public string keyword = "Grass"; //public string[] keywords = new string[1] { "Grass" };
            [SerializeField]
            [HideInInspector]
            internal string lowerKeyword = "grass";

            public STSettings settings = new STSettings();
        }

        private void OnValidate()
        {
            for (int i = 0; i < subTypes.Length; i++)
            {
                var st = subTypes[i];
                st.lowerKeyword = st.keyword.ToLowerInvariant();
            }
        }
    }
}