using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Tile Class", menuName = "Tile Class")]
public class TileClass : ScriptableObject
{
    public string tileName;
    //public Sprite tileSprite;
    public Sprite[] tileSprites; // use different sprites for each tile tpo create a variety
}
