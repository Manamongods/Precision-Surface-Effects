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
public class ArraycastTester : MonoBehaviour
{
    //Fields
    [Min(1)]
    public int maxOutputCount = 4;
    public float minWeight = 0.2f;

    [Header("Input")]
    [Space(50)]
    public SurfaceData surfaceData;

    public float width = 1;
    public float depth = 1;
    public float yStep = -5;
    public float yOffset = 0;
    public int count; //public float mult = 1;


    //Lifecycle
    private void Awake()
    {
        if (Application.isPlaying)
            enabled = false;
    }

    private void Update()
    {
        var pos = transform.position;

        var right = transform.TransformVector(Vector3.right);
        var down = transform.TransformVector(Vector3.down);
        var forward = transform.TransformVector(Vector3.forward);

        for (int i = 0; i < count; i++)
        {
            float t = i / (count - 1f);
            var pos2 = pos + right * (t - 0.5f) * width;

            SurfaceOutputs outputs = surfaceData.GetRaycastSurfaceTypes(pos2, down, maxOutputCount: maxOutputCount, shareList: true);
            outputs.Downshift(maxOutputCount, minWeight); //, mult);

            for (int ii = 0; ii < outputs.Count; ii++)
            {
                var output = outputs[ii];

                var pos3 = pos2 - down * yStep * (yOffset  + output.surfaceTypeID);
                Debug.DrawLine(pos3, pos3 + forward * output.weight * depth, Color.yellow);
            }
        }
    }
}
#endif