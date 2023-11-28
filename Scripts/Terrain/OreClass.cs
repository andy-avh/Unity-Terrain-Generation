using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class OreClass
{
    // Ores stored in array on TerrainGeneration script
    // Coal = 0
    // Iron = 1
    // Gold = 2
    // Diamond = 3
    // each ore needs:
    public string name;
    [Range(0,1)]
    public float rarity;
    [Range(0, 1)]
    public float size;
    public int maxSpawnHeight;
    public Texture2D spreadTexture;
}
