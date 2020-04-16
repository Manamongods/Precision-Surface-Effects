/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

[ExecuteInEditMode]
public class CastSoundPerformanceTester : MonoBehaviour
{
    //Fields
    public UnityEngine.UI.Text text;

    [Min(1)]
    [Space(30)]
    public int maxOutputCount = 4;
    public float minWeight = 0.2f;

    [Header("Input")]
    [Space(30)]
    public SurfaceSoundSet soundSet;

    public bool reuseRaycastHit;
    public int times = 1000;
    public int smoothFrames = 100;

    private AudioClip clip;
    private float volume;
    private float pitch;

    private float time;

    private List<float> elapses = new List<float>();



    //Lifecycle
    private void Awake()
    {
        if (Application.isPlaying)
            enabled = false;
    }

    private void Update()
    {
        var pos = transform.position;
        var downDir = -transform.up;

        System.Diagnostics.Stopwatch sw = new System.Diagnostics.Stopwatch();
        sw.Start();

        if (reuseRaycastHit)
        {
            if (!Physics.Raycast(pos, downDir, out RaycastHit rh, Mathf.Infinity))
                return;

            for (int i = 0; i < times; i++)
            {
                SurfaceOutputs outputs = soundSet.data.GetRHSurfaceTypes(rh, shareList: true);
                outputs.Downshift(maxOutputCount, minWeight);

                for (int ii = 0; ii < outputs.Count; ii++)
                    clip = soundSet.surfaceTypeSounds[outputs[ii].surfaceTypeID].GetRandomClip(out volume, out pitch);
            }
        }
        else
        {
            for (int i = 0; i < times; i++)
            {
                SurfaceOutputs outputs = soundSet.data.GetRaycastSurfaceTypes(pos, downDir, shareList: true);
                outputs.Downshift(maxOutputCount, minWeight);

                for (int ii = 0; ii < outputs.Count; ii++)
                    clip = soundSet.surfaceTypeSounds[outputs[ii].surfaceTypeID].GetRandomClip(out volume, out pitch);
            }
        }

        sw.Stop();
        float time = (float)sw.Elapsed.TotalMilliseconds;

        elapses.Insert(0, time);
        while (elapses.Count > smoothFrames)
            elapses.RemoveAt(elapses.Count - 1);

        float sum = 0;
        for (int i = 0; i < elapses.Count; i++)
            sum += elapses[i];
        sum /= smoothFrames;

        this.text.text = times + (reuseRaycastHit ? " Reused RH " : "") + " Iterations: \n\n" + time.ToString("00.00") + " MS\n\n" + sum.ToString("00.00") + " MS";
    }
}