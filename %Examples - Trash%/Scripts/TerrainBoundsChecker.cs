using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TerrainBoundsChecker : MonoBehaviour
{
    private void OnDrawGizmosSelected()
    {
        var terrain = GetComponent<Terrain>();

        var terrainData = terrain.terrainData; //terrain = Terrain.activeTerrain;
        var terrainPos = terrain.transform.position;

        //float mapX = (((WorldPos.x - terrainPos.x) / terrainData.size.x) * terrainData.alphamapWidth);
        //float mapZ = (((WorldPos.z - terrainPos.z) / terrainData.size.z) * terrainData.alphamapHeight);


        float down = -10000;
        Vector3 up = Vector3.up * 20000;

        Gizmos.DrawRay(new Vector3(terrainPos.x, down, terrainPos.z), up);
        Gizmos.DrawRay(new Vector3(terrainPos.x, down, terrainPos.z + terrainData.size.z), up);
        Gizmos.DrawRay(new Vector3(terrainPos.x + terrainData.size.x, down, terrainPos.z + terrainData.size.z), up);
        Gizmos.DrawRay(new Vector3(terrainPos.x + terrainData.size.x, down, terrainPos.z), up);


        UnityEditor.SceneView sv = UnityEditor.SceneView.lastActiveSceneView;

        Vector3 WorldPos = sv != null ? sv.camera.transform.position : Vector3.zero;

        float mapX = (((WorldPos.x - terrainPos.x) / terrainData.size.x) * (terrainData.alphamapWidth - 1));
        float mapZ = (((WorldPos.z - terrainPos.z) / terrainData.size.z) * (terrainData.alphamapHeight - 1));

        int xx = (int)mapX;
        int zz = (int)mapZ;

        int ww = 1;
        for (int x = -ww; x <= ww; x++)
        {
            for (int z = -ww; z <= ww; z++)
            {
                var xoff = terrainData.size.x * ((xx + x) / ((float)terrainData.alphamapWidth - 1));
                var zoff = terrainData.size.z * ((zz + z) / ((float)terrainData.alphamapHeight - 1));
                Gizmos.DrawRay(new Vector3(terrainPos.x + xoff, down, terrainPos.z + zoff), up);
            }
        }
    }
}
