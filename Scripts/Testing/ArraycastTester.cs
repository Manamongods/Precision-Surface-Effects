/////////////////////////////////////////////////////////
//MIT License
//Copyright (c) 2020 Steffen Vetne
/////////////////////////////////////////////////////////

using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using PrecisionSurfaceEffects;

[ExecuteInEditMode]
[AddComponentMenu("")]
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

            SurfaceOutputs outputs = surfaceData.GetRaycastSurfaceTypes(pos2, down, shareList: true); //maxOutputCount: maxOutputCount, 
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