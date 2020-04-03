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
public class CastSoundTester : MonoBehaviour
{
    //Fields
    public UnityEngine.UI.Text text;

    [Header("Output")]
    [Space(30)]
    public TestResult[] results;

    [Min(1)]
    [Space(30)]
    public int maxOutputCount = 4;
    public float minWeight = 0.2f;

    [Header("Input")]
    [Space(30)]
    public SurfaceSoundSet soundSet;

    public bool downIsGravity;

    public bool spherecast;
    public float spherecastRadius = 5;

    [System.NonSerialized]
    public float mult = 1;


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
    private void Update()
    {
        var pos = transform.position;
        var downDir = GetDownDir();

        SurfaceOutputs outputs;
        if (spherecast)
            outputs = soundSet.data.GetSphereCastSurfaceTypes(pos, downDir, spherecastRadius, maxOutputCount: maxOutputCount, shareList: true);
        else
            outputs = soundSet.data.GetRaycastSurfaceTypes(pos, downDir, maxOutputCount: maxOutputCount, shareList: true);
        outputs.Downshift(maxOutputCount, minWeight, mult);

        results = new TestResult[outputs.Count];
        for (int i = 0; i < outputs.Count; i++)
        {
            var output = outputs[i];

            var s = soundSet.surfaceTypeSounds[output.surfaceTypeID];

            var r = results[i] = new TestResult();

            r.header = s.name;
            r.normalizedWeight = output.volume;
            r.clip = s.GetRandomClip(out r.volume, out r.pitch);
        }

        string text = "";
        for (int i = 0; i < results.Length; i++)
        {
            var result = results[i];
            text = text + result.header + " " + (Mathf.Round(result.normalizedWeight * 1000f) / 1000f) + "\n"; //" W: " + 
        }
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
#endif