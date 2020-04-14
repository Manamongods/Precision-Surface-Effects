using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MaxRealVoicer : MonoBehaviour
{
    public int maxCount = 64;

    private void Start()
    {
        var config = AudioSettings.GetConfiguration();
        config.numRealVoices = maxCount;
        AudioSettings.Reset(config);
    }
}