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
public class ArraycastSoundTester : MonoBehaviour
{
    //Fields
    [Header("Results")]
    public TestResults[] results;

    [Min(1)]
    public int maxOutputCount = 4;
    public float minWeight = 0.2f;

    [Header("Input")]
    [Space(50)]
    public SurfaceSoundSet soundSet;

    public float width = 1;
    public float depth = 1;
    public float yOffset = 1;
    public int count;

    public float mult = 1;



    //Datatypes
    [System.Serializable]
    public class TestResults
    {
        public SoundTester.TestResult[] results;
    }


    //Lifecycle
    private void Update()
    {
        var pos = transform.position;

        var right = transform.TransformVector(Vector3.right);
        var down = transform.TransformVector(Vector3.down);
        var forward = transform.TransformVector(Vector3.forward);

        results = new TestResults[count];
        for (int ii = 0; ii < count; ii++)
        {
            var rr = results[ii] = new TestResults();

            float t = ii / (count - 1f);
            var pos2 = pos + right * (t - 0.5f) * width;
            SurfaceOutputs outputs = soundSet.data.GetRaycastSurfaceTypes(pos2, down, maxOutputCount: maxOutputCount, shareList: true);
            outputs.Downshift(maxOutputCount, minWeight, mult);

            rr.results = new SoundTester.TestResult[outputs.Count];
            for (int i = 0; i < outputs.Count; i++)
            {
                var output = outputs[i];

                var s = soundSet.surfaceTypeSounds[output.surfaceTypeID];

                var r = rr.results[i] = new SoundTester.TestResult();

                r.header = s.name;
                r.normalizedWeight = output.normalizedWeight;
                r.clip = s.GetRandomClip(out r.volume, out r.pitch);

                var pos3 = pos2 - down * yOffset * i;
                Debug.DrawLine(pos3, pos3 + forward * output.normalizedWeight * depth, Color.yellow);
            }
        }
    }
}

/*
* 
        
*/
#endif