#if UNITY_EDITOR
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[EditorOnly]
[ExecuteInEditMode]
public class SceneCameraFollower : MonoBehaviour
{
    //private void OnEnable()
    //{
    //    UnityEditor.EditorApplication.update -= Update;
    //    UnityEditor.EditorApplication.update += Update;
    //}
    //private void OnDisable()
    //{
    //    UnityEditor.EditorApplication.update -= Update;
    //}

    private void Update()
    {
        var sv = UnityEditor.SceneView.lastActiveSceneView;
        if(sv != null)
        {
            var t = sv.camera.transform;
            transform.position = t.position;
            transform.rotation = t.rotation;
        }
    }
}
#endif