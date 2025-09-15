using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexamap;

public class StructureInfo
{
    public List<Tile> NeighborTiles { get; }
    public List<Tile> ColleagueTiles { get; }
    public GameObject StructureObject { get; }

    public EStructure StructureType { get; }

    public StructureInfo(List<Tile> _neighborTiles, List<Tile> _colleagueTiles, GameObject _structureObject, EStructure _structureType)
    {
        NeighborTiles = _neighborTiles;
        ColleagueTiles = _colleagueTiles;
        StructureObject = _structureObject;
        StructureType = _structureType;
    }
}
