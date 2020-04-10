using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace PrecisionSurfaceEffects
{
    public static class Utility
    {
        //(Highly modified), From: https://answers.unity.com/questions/456973/getting-the-texture-of-a-certain-point-on-terrain.html
        public static float[] GetTextureMix(Terrain terrain, Vector3 WorldPos)
        {
            //maybe a problem wtih out of bounds??!?!:


            var terrainData = terrain.terrainData; //terrain = Terrain.activeTerrain;
            var terrainPos = terrain.transform.position;

            // returns an array containing the relative mix of textures
            // on the main terrain at this world position.

            // The number of values in the array will equal the number
            // of textures added to the terrain.

            // calculate which splat map cell the worldPos falls within (ignoring y)
            float mapX = (((WorldPos.x - terrainPos.x) / terrainData.size.x) * (terrainData.alphamapWidth - 1));
            float mapZ = (((WorldPos.z - terrainPos.z) / terrainData.size.z) * (terrainData.alphamapHeight - 1));

            int mapXID = (int)mapX;
            int mapZID = (int)mapZ;

            float xT = mapX - mapXID;
            float zT = mapZ - mapZID;

            // get the splat data for this cell as a 1x1xN 3d array (where N = number of textures)
            float[,,] splatmapData = terrainData.GetAlphamaps(mapXID, mapZID, 2, 2);

            // extract the 3D array data to a 1D array:
            float[] cellMix = new float[splatmapData.GetUpperBound(2) + 1];

            for (int n = 0; n < cellMix.Length; n++)
            {
                //Dunno why but it seems that it is y, x, not x, y??? TODO: WHY THE FUCK?!
                float lowerMix = splatmapData[0, 0, n] * (1 - xT) + splatmapData[0, 1, n] * xT;
                float upperMix = splatmapData[1, 0, n] * (1 - xT) + splatmapData[1, 1, n] * xT;
                cellMix[n] = lowerMix * (1 - zT) + upperMix * zT;
            }
            return cellMix;
        }


        public static int GetSubmesh(Mesh mesh, int raycastHitTriangleIndex)
        {
            var triIndex = raycastHitTriangleIndex * 3;

            for (int submeshID = mesh.subMeshCount - 1; submeshID >= 0; submeshID--)
            {
                int start = mesh.GetSubMesh(submeshID).indexStart;

                if (triIndex >= start)
                {
                    return submeshID;
                }
            }

            throw new System.Exception("Is this even possible?");
        }


        public static void Fill<T>(GameObject g, ref T[] children) where T : Component
        {
            if (children == null || children.Length == 0)
                children = g.GetComponentsInChildren<T>();
        }


        public static Vector3 GetVelocityMass(Rigidbody r, Vector3 point, out float mass)
        {
            if (r == null)
            {
                mass = 1E32f; // float.MaxValue; // Mathf.Infinity;
                return Vector3.zero;
            }
            else
            {
                mass = r.mass;
                return r.GetPointVelocity(point);
            }
        }
    }
}

/*
 *         public static int GetMainTexture(float[] mixes, out float maxMix, float totalMax)
        {
            maxMix = 0; //maxMix
            int maxIndex = -1;

            // loop through each mix value and find the maximum
            for (int n = 0; n < mixes.Length; n++)
            {
                var m = mixes[n];
                if (m > maxMix && m < totalMax)
                {
                    maxIndex = n;
                    maxMix = m;
                }
            }
            return maxIndex;
        }
 */
