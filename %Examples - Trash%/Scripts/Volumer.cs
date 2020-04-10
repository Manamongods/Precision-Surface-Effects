using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Volumer : MonoBehaviour
{
    public float volume = 1;

    private void Update()
    {
        AudioListener.volume = volume;
    }
}
