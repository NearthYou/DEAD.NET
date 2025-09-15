using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexamap;

public class StructureObject : MonoBehaviour
{
    Tile curTile;

    public Tile CurTile => curTile;

    public void Init(Tile _tile)
    {
        curTile = _tile;
    }
}