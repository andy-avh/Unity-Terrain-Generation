using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Atlas", menuName = "Tile Atlas")]
public class TileAtlas : ScriptableObject
{
    public TileClass grass;
    public TileClass dirt;
    public TileClass gravel;
    public TileClass stone;
    public TileClass log;
    public TileClass leaf;

    // addons
    public TileClass tallGrass;

    // ores
    public TileClass coal;
    public TileClass ironOre;
    public TileClass goldOre;
    public TileClass diamond;
}
