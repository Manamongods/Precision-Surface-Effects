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

#if UNITY_EDITOR
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

        for (int i = 0; i < times; i++)
        {
            SurfaceOutputs outputs = soundSet.data.GetRaycastSurfaceTypes(pos, downDir, shareList: true);
            outputs.Downshift(maxOutputCount, minWeight);

            for (int ii = 0; ii < outputs.Count; ii++)
                clip = soundSet.surfaceTypeSounds[outputs[ii].surfaceTypeID].GetRandomClip(out volume, out pitch);
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

        this.text.text = times + " Iterations: \n\n" + time.ToString("00.00") + " MS\n\n" + sum.ToString("00.00") + " MS";
    }
}
#endif