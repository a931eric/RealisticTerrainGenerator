using System.Collections;
using System.Collections.Generic;
using UnityEditor.Experimental.SceneManagement;
using UnityEngine;

namespace RealisticTerrainGenerator
{
    [ExecuteInEditMode]
    public class Chunk : MonoBehaviour
    {
        MeshFilter meshFilter;
        public MeshRenderer mr;
        Mesh mesh;
        public MeshCollider meshCollider;
        public int posX, posY, w, h,W,H;
        Vector3[] vertices;
        public bool useCollider;

        public void Init()
        {
            if (PrefabStageUtility.GetPrefabStage(gameObject) != null) { return; }
            if (Application.isPlaying) { enabled = false; return; }
            meshCollider = GetComponent<MeshCollider>();meshCollider.sharedMesh = mesh;
            transform.localPosition = new Vector3(posX, 0, posY);
            mr = GetComponent<MeshRenderer>();
            meshFilter = GetComponent<MeshFilter>();
            meshFilter.mesh = mesh = new Mesh();
            
            vertices = new Vector3[h*w];
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    vertices[x+y*w] = new Vector3(x, 0, y);
                }
            }
            mesh.vertices = vertices;
            int[] triangles = new int[(h - 1) * (w - 1) * 6];
            int i = 0, j = 0;
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    if (x < w - 1 && y < h - 1)
                    {
                        triangles[6 * j] = i;
                        triangles[6 * j + 1] = i + w;
                        triangles[6 * j + 2] = i + 1 + w;
                        triangles[6 * j + 3] = i;
                        triangles[6 * j + 4] = i + 1 + w;
                        triangles[6 * j + 5] = i + 1;
                        j++;
                    }
                    i++;
                }
            }
            mesh.triangles = triangles;
            //UpdateHeight();
        }
        
        public void UpdateHeight(float [,] heightMap)
        {
            
            for (int y = 0; y < h; y++)
            {
                for (int x = 0; x < w; x++)
                {
                    Vector3 temp=new Vector3(x, heightMap[posY+y,posX+x], y);
                    vertices[x + y * w] = temp;
                }
            }
            mesh.vertices = vertices;
            mesh.RecalculateNormals(); 
            mesh.RecalculateBounds();
            Vector2[] uvs = new Vector2[vertices.Length];
            for (int i = 0; i < uvs.Length; i++)
            {
                uvs[i] = new Vector2((posX+ vertices[i].x)/W,(posY+ vertices[i].z)/H);
            }
            mesh.uv = uvs;
            if (useCollider) meshCollider.sharedMesh = mesh;

        }
    }
}
