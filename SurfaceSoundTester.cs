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
    [Space(20)]
    public string soundSetName;

    [Header("Input")]
    [Space(50)]
    public SurfaceSounds sounds;
    public int soundSetID = 0;
    public bool downIsGravity;



    //Lifecycle
    private void Update()
    {
        soundSetID = Mathf.Clamp(soundSetID, 0, sounds.soundSetNames.Length - 1);
        soundSetName = sounds.soundSetNames[soundSetID];

        Vector3 downDir;
        if (downIsGravity)
            downDir = Physics.gravity;
        else
            downDir = -transform.up;

        var pos = transform.position;

        var st = sounds.GetSurfaceType(pos, downDir);
        header = st.header;
        clip = st.GetSoundSet(soundSetID).GetRandomClip(out volume, out pitch);
    }
}

/*
* 
        //if (Physics.SphereCast(pos, 0.01f, downDir, out RaycastHit rh))
            //Debug.DrawLine(pos, rh.point);
*/
#endif