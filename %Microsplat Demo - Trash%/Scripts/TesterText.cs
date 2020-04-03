using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TesterText : MonoBehaviour
{
    public UnityEngine.UI.Text text;
    public SoundTester ts;

    private void Update()
    {
        string s = "";

        for (int i = 0; i < ts.results.Length; i++)
        {
            var result = ts.results[i];
            s = s + result.header + " " + (Mathf.Round(result.normalizedWeight * 1000f) / 1000f) + "\n"; //" W: " + 
        }

        text.text = s;
    }
}