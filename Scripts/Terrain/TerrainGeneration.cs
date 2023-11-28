using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Atlas")]
    public TileAtlas tileAtlas;

    [Header("Tree Generation")]
    public int treeChance = 10; // percent chance for tree to spawn. 10 = 10% (every block on surface)
    public int minTreeHeight = 3;
    public int maxTreeHeight = 6;

    [Header("Addons")]
    public int tallGrassChance = 10;

    [Header("Terrain Generation")]
    public int chunkSize = 16; // Load world in chunks
    public float surfaceValue = 0.25f;
    public int worldSize = 100;
    public float heightMultiplier = 4f;
    public int heightAddition = 25; // smooth out hills

    [Header("Material Layers")]
    public int dirtLayerHeight = 5;
    public int gravelLayerHeight = 12;

    [Header("Noise Settings")]
    public float caveFreq = 0.05f;
    public float terrainFreq = 0.05f;
    public float seed;
    public Texture2D caveNoiseTexture; // basically a 2d array of colours, each index is a pixel of texture

    [Header("Ore Settings")]
    public OreClass[] ores;
/*    public float coalRarity;
    public float coalSize;
    public float ironRarity, ironSize;
    public float goldRarity, goldSize;
    public float diamondRarity, diamondSize;
    public Texture2D coalSpread;
    public Texture2D ironSpread;
    public Texture2D goldSpread;
    public Texture2D diamondSpread;*/

    private GameObject[] worldChunks;
    private List<Vector2> worldTiles = new List<Vector2>(); // keep track of generated tiles

    private void OnValidate()
    {
        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[3].spreadTexture = new Texture2D(worldSize, worldSize);
        
        // Generate cave pattern
        GenerateNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        // Generate ores
        GenerateNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture); // ... coal
        GenerateNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);  // ... iron
        GenerateNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);  // ... gold
        GenerateNoiseTexture(ores[3].rarity, ores[3].size, ores[3].spreadTexture); // ... diamond
    }

    private void Start()
    {
        seed = Random.Range(-10000, 10000);

        caveNoiseTexture = new Texture2D(worldSize, worldSize);
        ores[0].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[1].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[2].spreadTexture = new Texture2D(worldSize, worldSize);
        ores[3].spreadTexture = new Texture2D(worldSize, worldSize);

        // Generate cave pattern
        GenerateNoiseTexture(caveFreq, surfaceValue, caveNoiseTexture);
        // Generate ores
        GenerateNoiseTexture(ores[0].rarity, ores[0].size, ores[0].spreadTexture); // ... coal
        GenerateNoiseTexture(ores[1].rarity, ores[1].size, ores[1].spreadTexture);  // ... iron
        GenerateNoiseTexture(ores[2].rarity, ores[2].size, ores[2].spreadTexture);  // ... gold
        GenerateNoiseTexture(ores[3].rarity, ores[3].size, ores[3].spreadTexture); // ... diamond
        CreateChunks();
        GenerateTerrain();
    }

    public void CreateChunks()
    {
        int numChuncks = worldSize / chunkSize; // number of chunks generated
        worldChunks = new GameObject[numChuncks];

        for(int i = 0; i<numChuncks; i++ )
        {
            GameObject newChunk = new GameObject();
            newChunk.name = i.ToString(); // name chunks in hierachy
            newChunk.transform.parent = this.transform;
            worldChunks[i] = newChunk; // add chunks to array of chunks
        }
    }

    public void GenerateTerrain()
    {

        // male world actually look like terrain
        for (int x = 0; x < worldSize; x++) // generate to world size on x...
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for (int y = 0; y < height; y++) // ...and y on height
            {
                // Generate world
                Sprite[] tileSprites;
                if (y < height - gravelLayerHeight)
                {
                    tileSprites = tileAtlas.stone.tileSprites; // ... stone

                    // Generate Ores
                    // go in levels of rarity
                    if (ores[0].spreadTexture.GetPixel(x,y).r > 0.5f && height - y > ores[0].maxSpawnHeight) // ... coal
                        tileSprites = tileAtlas.coal.tileSprites;
                    if (ores[1].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[1].maxSpawnHeight)    // ... iron
                        tileSprites = tileAtlas.ironOre.tileSprites;
                    if(ores[2].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[2].maxSpawnHeight)  // ... gold
                        tileSprites = tileAtlas.goldOre.tileSprites;
                    if(ores[3].spreadTexture.GetPixel(x, y).r > 0.5f && height - y > ores[3].maxSpawnHeight) //  if we dont want diamond in stone, move line to section generating different material (blackstone, bedrock etc)
                        tileSprites = tileAtlas.diamond.tileSprites;      // ... diamond

                }
                else if(y < height - dirtLayerHeight)
                {
                    tileSprites = tileAtlas.gravel.tileSprites; // ... gravel
                }
                else if (y < height - 1)
                {
                    tileSprites = tileAtlas.dirt.tileSprites; // ... dirt
                }
                else
                {
                    // top layer of terrain
                    tileSprites = tileAtlas.grass.tileSprites; // ... grass
                }

                // This section generates caves
                if (caveNoiseTexture.GetPixel(x, y).r > 0.5f)
                {
                    PlaceTile(tileSprites, x, y);
                }

                if(y > height - 1)
                {
                    // SPAWN TREES //
                    int t = Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        // generate a tree
                        if (worldTiles.Contains(new Vector2(x, y))) // only spawn tree if above grass (don't want floating trees)
                        {
                            GenerateTree(x, y + 1); // placed one block above grass
                        }
                    }
                    else // SPAWN GRASS //
                    {
                        int i = Random.Range(0, tallGrassChance);
                        // generate grass
                        if(i == 1)
                        {
                            if (worldTiles.Contains(new Vector2(x, y))) // only spawn tree if above grass (don't want floating trees)
                            {
                                PlaceTile(tileAtlas.tallGrass.tileSprites, x, y + 1); // placed one block above grass
                            }
                        } 
                    }
                }
            }
        }
    }

    public void GenerateNoiseTexture(float frequency, float limit,Texture2D noiseTexture)
    {
        // look through each pixel
        for(int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed)* frequency, (y + seed) * frequency);
                if(v > limit)
                {
                    noiseTexture.SetPixel(x, y, Color.white);
                }
                else
                {
                    noiseTexture.SetPixel(x, y, Color.black);
                }
               
            }
        }

        noiseTexture.Apply();
    }

    public void GenerateTree(int x, int y)
    {
        // define our tree trunk
        int treeHeight = Random.Range(minTreeHeight, maxTreeHeight); // tree height will be different for each tree
        for(int i = 0; i < treeHeight; i ++)
        {
            PlaceTile(tileAtlas.log.tileSprites, x, y + i);
        }

        // generate leaves for trees
        PlaceTile(tileAtlas.leaf.tileSprites, x, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites, x, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.tileSprites, x, y + treeHeight + 2);

        PlaceTile(tileAtlas.leaf.tileSprites, x - 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites, x - 1, y + treeHeight + 1);
        PlaceTile(tileAtlas.leaf.tileSprites, x + 1, y + treeHeight);
        PlaceTile(tileAtlas.leaf.tileSprites, x + 1, y + treeHeight + 1);
    }

    public void PlaceTile(Sprite[] tileSprites, int x, int y)
    {
        GameObject newTile = new GameObject(); // NEED TO SAVE THIS GAMEOBJECTS STATE, THEN LOAD IT BACK IN WHEN WE START PLAYING AGIN

        // Place chunks in correct place
        float chunkCoord = (Mathf.RoundToInt(x / chunkSize) * chunkSize);
        chunkCoord /= chunkSize;
        newTile.transform.parent = worldChunks[(int)chunkCoord].transform;

        // Place tile sprites
        newTile.AddComponent<SpriteRenderer>();

        int spriteIndex = Random.Range(0, tileSprites.Length);
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprites[spriteIndex];
        newTile.name = tileSprites[0].name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f)); // track all tiles added to get their position
    }
}

/*
if (y < height - dirtLayerHeight)
{
    tileSprite = stone;
}
else if (y < height - 1)
{
    tileSprite = dirt;
}
else
{
    // top layer of terrain
    tileSprite = grass;
}
*/
