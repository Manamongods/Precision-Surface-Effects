//using System.Collections;
//using System.Collections.Generic;
//using UnityEngine;

//public class BasicSurfaceSound : MonoBehaviour
//{
//    //Fields
//    public SurfaceSoundSet soundSet;
//    public SurfaceParticleSet particleSet;

//    public float radius = 0;
//    public float countScaler = 1;
//    public float sizeScaler = 1;

//    public AudioSource[] audioSources;
//    public float minimumWeight = 0.1f;

//    public float volumeMultiplier = 1;
//    public float pitchMultiplier = 1;


//    //Methods
//    public void Play()
//    {
//        var outputs = soundSet.data.GetRaycastSurfaceTypes(transform.position, -transform.up);
//        outputs.Downshift(audioSources.Length, minimumWeight);

//        if (outputs.collider != null)
//        {
//            var rb = outputs.collider.attachedRigidbody;
//            if (rb != null)
//                rb.AddForceAtPosition(-outputs.hitNormal * impulse, outputs.hitPosition, ForceMode.Impulse); //dir 
//        }

//        for (int i = 0; i < outputs.Count; i++)
//        {
//            var output = outputs[i];

//            soundSet.PlayOneShot(output, audioSources[i], volumeMultiplier, pitchMultiplier);

//            particleSet.PlayParticles(outputs, output, impulse, speed * dir, radius, deltaTime: 0.25f); //-speed * outputs.hitNormal
//            //Deltatime allows you to control how many max particles you can accept, because Time.deltaTime is irrelevant for a single shot
//        }
//    }


//    //Lifecycle
//    private void Update()
//    {
//        if (Input.GetKeyDown(KeyCode.R))
//        {
//            var r = Camera.main.ScreenPointToRay(Input.mousePosition);
//            Shoot(r.origin, r.direction);
//        }
//    }
//}
