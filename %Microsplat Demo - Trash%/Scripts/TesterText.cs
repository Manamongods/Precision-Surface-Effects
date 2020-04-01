using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[ExecuteInEditMode]
public class TesterText : MonoBehaviour
{
    public UnityEngine.UI.Text text;
    public SurfaceSoundTester ts;

    private void Update()
    {
        text.text = ts.header;
    }
}
