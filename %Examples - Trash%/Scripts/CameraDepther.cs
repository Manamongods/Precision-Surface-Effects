using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class CameraDepther : MonoBehaviour
{
    private void OnEnable()
    {
        GetComponent<Camera>().depthTextureMode = DepthTextureMode.Depth;
    }
}
