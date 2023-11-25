using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Random = UnityEngine.Random;

public class TerrainGeneration : MonoBehaviour
{
    [Header("Tile Sprites")]
    public Sprite grass;
    public Sprite dirt;
    public Sprite stone;
    public Sprite log;
    public Sprite leaf;

    [Header("Tree Generation")]
    public int treeChance = 10; // percent chance for tree to spawn. 10 = 10% (every block on surface)
    public int minTreeHeight = 3;
    public int maxTreeHeight = 6;

    [Header("Terrain Generation")]
    public int dirtLayerHeight = 5;
    public float surfaceValue = 0.25f;
    public int worldSize = 100;
    public float heightMultiplier = 4f;
    public int heightAddition = 25; // smooth out hills

    [Header("Noise Settings")]
    public float caveFreq = 0.05f;
    public float terrainFreq = 0.05f;
    public float seed;
    public Texture2D noiseTexture; // basically a 2d array of colours, each index is a pixel of texture

    private List<Vector2> worldTiles = new List<Vector2>(); // keep track of generated tiles

    private void Start()
    {
        seed = Random.Range(-10000, 10000);
        GenerateNoiseTexture();
        GenerateTerrain();
    }

    public void GenerateTerrain()
    {
        // male world actually look like terrain
        for (int x = 0; x < worldSize; x++) // generate to world size on x...
        {
            float height = Mathf.PerlinNoise((x + seed) * terrainFreq, seed * terrainFreq) * heightMultiplier + heightAddition;

            for (int y = 0; y < height; y++) // ...and y on height
            {
                Sprite tileSprite; ;
                if (y < height - dirtLayerHeight)
                {
                    tileSprite = stone;
                }
                else if(y < height - 1)
                {
                    tileSprite = dirt;
                }
                else
                {
                    // top layer of terrain
                    tileSprite = grass;

                    
                }

                // This section generates caves
                if (noiseTexture.GetPixel(x, y).r > surfaceValue)
                {
                    PlaceTile(tileSprite, x, y);
                }

                if(y > height - 1)
                {
                    // spawn trees
                    int t = Random.Range(0, treeChance);
                    if (t == 1)
                    {
                        // generate a tree
                        if (worldTiles.Contains(new Vector2(x, y))) // only spawn tree if above grass (don't want floating trees)
                        {
                            GenerateTree(x, y + 1); // placed one bloack above grass
                        }

                    }
                }
            }
        }
    }

    public void GenerateNoiseTexture()
    {
        // define size of texture
        noiseTexture = new Texture2D(worldSize, worldSize);
        // look through each pixel
        for(int x = 0; x < noiseTexture.width; x++)
        {
            for (int y = 0; y < noiseTexture.height; y++)
            {
                float v = Mathf.PerlinNoise((x + seed)* caveFreq, (y + seed) * caveFreq);
                noiseTexture.SetPixel(x, y, new Color(v, v, v));
            }
        }

        noiseTexture.Apply();
    }

    public void GenerateTree(float x, float y)
    {
        // define our tree trunk
        int treeHeight = Random.Range(minTreeHeight, maxTreeHeight); // tree height will be different for each tree
        for(int i = 0; i < treeHeight; i ++)
        {
            PlaceTile(log, x, y + i);
        }

        // generate leaves for trees
        PlaceTile(leaf, x, y + treeHeight);
        PlaceTile(leaf, x, y + treeHeight + 1);
        PlaceTile(leaf, x, y + treeHeight + 2);

        PlaceTile(leaf, x - 1, y + treeHeight);
        PlaceTile(leaf, x - 1, y + treeHeight + 1);
        PlaceTile(leaf, x + 1, y + treeHeight);
        PlaceTile(leaf, x + 1, y + treeHeight + 1);
    }

    public void PlaceTile(Sprite tileSprite, float x, float y)
    {
        GameObject newTile = new GameObject(); // NEED TO SAVE THIS GAMEOBJECTS STATE, THEN LOAD IT BACK IN WHEN WE START PLAYING AGIN
        newTile.transform.parent = this.transform;
        newTile.AddComponent<SpriteRenderer>();
        newTile.GetComponent<SpriteRenderer>().sprite = tileSprite;
        newTile.name = tileSprite.name;
        newTile.transform.position = new Vector2(x + 0.5f, y + 0.5f);

        worldTiles.Add(newTile.transform.position - (Vector3.one * 0.5f)); // track all tiles added to get their position
    }
}
