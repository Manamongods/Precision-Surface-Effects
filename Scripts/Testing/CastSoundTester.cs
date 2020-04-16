/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

[ExecuteInEditMode]
public class CastSoundTester : MonoBehaviour
{
    //Fields
    public UnityEngine.UI.Text text;

    [Header("Output")]
    [Space(30)]
    public TestResult[] results;
    public float hardness;

    [Min(1)]
    [Space(30)]
    public int maxOutputCount = 4;
    public float minWeight = 0.2f;

    [Header("Input")]
    [Space(30)]
    public SurfaceSoundSet soundSet;

    public bool downIsGravity;

    public bool spherecast;
    public float spherecastRadius = 5; //public float mult = 1;


    //Methods
    private Vector3 GetDownDir()
    {
        if (downIsGravity)
            return Physics.gravity;
        else
            return -transform.up;
    }


    //Datatypes
    [System.Serializable]
    public class TestResult
    {
        public string header;
        public float normalizedWeight;
        public AudioClip clip;
        public float volume;
        public float pitch;
    }


    //Lifecycle
    private void Awake()
    {
        if (Application.isPlaying)
            enabled = false;
    }

    private void Update()
    {
        var pos = transform.position;
        var downDir = GetDownDir();

        SurfaceOutputs outputs;
        if (spherecast)
            outputs = soundSet.data.GetSphereCastSurfaceTypes(pos, downDir, spherecastRadius, shareList: true); //maxOutputCount: maxOutputCount, 
        else
            outputs = soundSet.data.GetRaycastSurfaceTypes(pos, downDir, shareList: true);
        outputs.Downshift(maxOutputCount, minWeight); //, mult);

        hardness = outputs.hardness;

        results = new TestResult[outputs.Count];
        for (int i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];

            var s = soundSet.surfaceTypeSounds[output.surfaceTypeID];

            var r = results[i] = new TestResult();

            r.header = s.name;
            r.normalizedWeight = output.weight;
            r.clip = s.GetRandomClip(out r.volume, out r.pitch);
        }

        string text = "";
        for (int i = 0; i < results.Length; i++)
        {
            var result = results[i];
            text = text + result.header + " " + (Mathf.Round(result.normalizedWeight * 1000f) / 1000f) + "\n"; //" W: " + 
        }
        text = text + "\nHardness: " + (Mathf.Round(hardness * 1000f) / 1000f);
        this.text.text = text;
    }

    private void OnDrawGizmos()
    {
        if (spherecast)
        {
            Gizmos.DrawWireSphere(transform.position, spherecastRadius);

            var pos = transform.position;
            var dir = GetDownDir();
            if (Physics.SphereCast(pos, spherecastRadius, dir, out RaycastHit rh))
            {
                Gizmos.DrawWireSphere(pos + dir * rh.distance, spherecastRadius); //rh.point
            }
        }
    }
}

/*
* 
        
*/