using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using RealisticTerrainGenerator;
using Sirenix.OdinInspector;

public class SetLatent : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }
    public float scale;
    [Button()]
    void Set()
    {
        RealisticTerrain rt = GetComponent<RealisticTerrain>();
        for (int i = 0; i < 512; i++)
            for (int j = 0; j < 512; j++)
                rt.c[0].map[i, j] = (j - 256) / 25.6f* scale;
        for (int i = 0; i < 512; i++)
            for (int j = 0; j < 512; j++)
                rt.c[1].map[i, j] = (i - 256) / 25.6f* scale;
    }
}
