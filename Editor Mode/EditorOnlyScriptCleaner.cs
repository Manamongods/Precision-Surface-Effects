using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System;
#if UNITY_EDITOR
using UnityEditor;
using UnityEditor.Build;
using UnityEditor.Build.Reporting;
using UnityEngine.SceneManagement;
#endif

[AttributeUsage(AttributeTargets.Class, Inherited = false)]
public class EditorOnlyAttribute : Attribute
{
    
}

#if UNITY_EDITOR
public class EditorOnlyScriptCleaner : IProcessSceneWithReport
{
    public int callbackOrder { get { return 0; } }

    public void OnProcessScene(Scene scene, BuildReport report)
    {
        var attribute = typeof(EditorOnlyAttribute);

        var monobehaviours = GameObject.FindObjectsOfType<MonoBehaviour>();

        for (int i = 0; i < monobehaviours.Length; i++)
        {
            var component = monobehaviours[i];

            if(component.GetType().IsDefined(attribute, false)) //true
                UnityEngine.Object.DestroyImmediate(component);
        }
    }
}
#endif