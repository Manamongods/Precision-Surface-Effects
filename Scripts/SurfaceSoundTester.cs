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

[ExecuteInEditMode]
public class SurfaceSoundTester : MonoBehaviour
{
    //Fields
    [Header("Result")]
    public string header;
    public AudioClip clip;
    public float volume;
    public float pitch;

    [Header("Input")]
    [Space(50)]
    public SurfaceSoundSet soundSet;
    public bool downIsGravity;

    public bool spherecast;
    public float spherecastRadius = 5;


    //Methods
    private Vector3 GetDownDir()
    {
        if (downIsGravity)
            return Physics.gravity;
        else
            return -transform.up;
    }


    //Lifecycle
    private void Update()
    {
        GetDownDir();

        var pos = transform.position;
        var downDir = GetDownDir();

        int sID;
        if (spherecast)
            sID = soundSet.surfaceTypes.GetSphereCastSurfaceTypeID(pos, downDir, spherecastRadius);
        else
            sID = soundSet.surfaceTypes.GetRaycastSurfaceTypeID(pos, downDir);

        var s = soundSet.surfaceTypeSounds[sID];

        header = s.autoGroupName;
        clip = s.GetRandomClip(out volume, out pitch);
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