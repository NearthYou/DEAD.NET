using System.Collections.Generic;
using System.Linq;
using System;
using UnityEngine;
using Hexamap;

public class StructureManager : MonoBehaviour
{
    [Header("구조물 설정")] [Space(5f)] [SerializeField]
    private Transform objectsTransform;
    [SerializeField] private MapPrefabSO mapPrefab;
    
    private MapController mapController;
    private List<Tile> preemptiveTiles = new List<Tile>();
    
    public List<Tile> PreemptiveTiles => preemptiveTiles;
    
    public void Initialize(MapController controller, Transform objectsParent, MapPrefabSO prefab)
    {
        mapController = controller;
        objectsTransform = objectsParent;
        mapPrefab = prefab;
    }
    
    public void GenerateTower()
    {
        var tilelist = new List<Tile>();
        Tile tile = mapController.GetTileFromCoords(new Coords(1, 3));
        tilelist.Add(tile);

        List<Tile> neighborList = SetNeighborStructure(tilelist);

        var spawnPos = ((GameObject)tile.GameEntity).transform.position;
        spawnPos.y += TOWER_SPAWN_HEIGHT;

        var tower = Instantiate(mapPrefab.items[(int)EMabPrefab.Tower].prefab, spawnPos, Quaternion.Euler(0, 90, 0),
            objectsTransform);

        tower.GetComponent<StructureObject>().Init(tile);
        ((GameObject)tile.GameEntity).GetComponent<TileBase>().SpawnQuestStructure(neighborList, tower);

        preemptiveTiles.Add(tile);
    }
    
    public void Generate7TileStructure(Coords coords)
    {
        var boundaryTiles = ObjectSpawnDistanceCalculate(PRODUCTION_DISTANCE);
        List<int> selectNumber = mapController.RandomTileSelect(boundaryTiles, EObjectSpawnType.ExcludeEntites, 1);

        var tilelist = new List<Tile>();
        GameObject structureObject = mapPrefab.items[(int)EMabPrefab.Production].prefab;
        Tile tile = boundaryTiles[selectNumber[0]];

        tilelist.Add(tile);
        tilelist.Add(tile.Neighbours[CompassPoint.N]);
        tilelist.Add(tile.Neighbours[CompassPoint.S]);
        tilelist.Add(tile.Neighbours[CompassPoint.NE]);
        tilelist.Add(tile.Neighbours[CompassPoint.SE]);
        tilelist.Add(tile.Neighbours[CompassPoint.NW]);
        tilelist.Add(tile.Neighbours[CompassPoint.SW]);

        foreach (var item in tilelist)
        {
            preemptiveTiles.Add(item);
        }

        List<Tile> neighborList = SetNeighborStructure(tilelist);

        var spawnPos = ((GameObject)tile.GameEntity).transform.position;
        spawnPos.y += PRODUCTION_SPAWN_HEIGHT;

        var structure = Instantiate(structureObject, spawnPos, Quaternion.Euler(0, 180, 0),
            objectsTransform);

        structure.GetComponent<StructureObject>().Init(tile);
        structure.name = "Production";

        StructureInfo structureInfo = new StructureInfo(neighborList, tilelist, structure, EStructure.Production);

        for (var index = 0; index < tilelist.Count; index++)
        {
            var tileBase = ((GameObject)tilelist[index].GameEntity).GetComponent<TileBase>();
            tileBase.SpawnNormalStructure(structureInfo);

            var position = tileBase.transform.position;
            position.y = ((GameObject)tile.GameEntity).transform.position.y;
            tileBase.transform.position = position;
        }
    }
    
    public void Generate3TileStructure(Coords coords)
    {
        var boundaryTiles = ObjectSpawnDistanceCalculate(ARMY_DISTANCE);
        List<int> selectNumber = mapController.RandomTileSelect(boundaryTiles, EObjectSpawnType.ExcludeEntites, 1);

        var tilelist = new List<Tile>();
        GameObject structureObject = mapPrefab.items[(int)EMabPrefab.Army].prefab;
        Tile tile = boundaryTiles[selectNumber[0]];

        tilelist.Add(tile);
        tilelist.Add(tile.Neighbours[CompassPoint.NW]);
        tilelist.Add(tile.Neighbours[CompassPoint.SW]);

        foreach (var item in tilelist)
        {
            preemptiveTiles.Add(item);
        }

        List<Tile> neighborList = SetNeighborStructure(tilelist);

        var spawnPos = ((GameObject)tile.GameEntity).transform.position;
        spawnPos.y += ARMY_SPAWN_HEIGHT;

        var structure = Instantiate(structureObject, spawnPos, Quaternion.Euler(0, 90, 0),
            objectsTransform);

        structure.GetComponent<StructureObject>().Init(tile);
        structure.name = "Army";
        StructureInfo structureInfo = new StructureInfo(neighborList, tilelist, structure, EStructure.Army);

        for (var index = 0; index < tilelist.Count; index++)
        {
            var tileBase = ((GameObject)tilelist[index].GameEntity).GetComponent<TileBase>();
            tileBase.SpawnNormalStructure(structureInfo);

            var position = tileBase.transform.position;
            position.y = ((GameObject)tile.GameEntity).transform.position.y;
            tileBase.transform.position = position;
        }
    }
    
    public void SpawnSpecialItemRandomTile(List<TileBase> tileBases)
    {
        int randomInt = UnityEngine.Random.Range(0, tileBases.Count);
        var randomTile = tileBases[randomInt];

        if (randomTile.Structure == null)
            Debug.Log("비어있음");

        randomTile.AddSpecialItem();
    }
    
    private List<Tile> ObjectSpawnDistanceCalculate(int range)
    {
        var tileList = mapController.GetAllTiles();

        int biggerInt = 0;
        int maxInt = 0;

        for (int i = 0; i < tileList.Count; i++)
        {
            Tile lastIndex = tileList[i];

            biggerInt = Math.Abs(lastIndex.Coords.X) > Math.Abs(lastIndex.Coords.Y)
                ? Math.Abs(lastIndex.Coords.X)
                : Math.Abs(lastIndex.Coords.Y);

            if (biggerInt > maxInt)
                maxInt = biggerInt;
        }

        List<Tile> excludeTileList = mapController.GetTilesInRange(mapController.GetTileFromCoords(new Coords(0, 0)), maxInt - range);
        return excludeTileList;
    }
    
    private List<Tile> SetNeighborStructure(List<Tile> tiles)
    {
        List<Tile> neighborTiles = new List<Tile>();

        for (int i = 0; i < tiles.Count; i++)
        {
            var tile = tiles[i];
            var list = tile.Neighbours.Select(kvp => kvp.Value).ToList();
            neighborTiles.AddRange(list);
        }

        neighborTiles = neighborTiles.Distinct().ToList();

        for (var index = 0; index < neighborTiles.Count; index++)
        {
            var tile = neighborTiles[index];
            ((GameObject)tile.GameEntity).GetComponent<TileBase>().SetNeighborStructure();
        }

        return neighborTiles;
    }
    
    public StructureBase SensingStructure(Player player)
    {
        var tileList = player.TileController.Model.Neighbours;

        foreach (var item in tileList)
        {
            if (mapController.LandformCheck(mapController.TileToTileController(item.Value)) == false)
                continue;

            var tileBase = ((GameObject)item.Value.GameEntity).GetComponent<TileBase>();

            if (tileBase.Structure != null)
                return tileBase.Structure;
        }

        return null;
    }
    
    public bool SensingSignalTower(Player player)
    {
        var structure = SensingStructure(player);
        return structure is Tower;
    }
    
    public bool SensingProductionStructure(Player player)
    {
        var structure = SensingStructure(player);
        return structure is ProductionStructure;
    }
    
    public StructureType GetStructureType(StructureBase structure)
    {
        if (structure is Tower) return StructureType.Tower;
        if (structure is ProductionStructure) return StructureType.Production;
        if (structure is ArmyStructure) return StructureType.Army;
        return StructureType.Tower; // 기본값
    }
    
    // 구조물 생성용 상수
    private const float TOWER_SPAWN_HEIGHT = 0.31f;
    private const float PRODUCTION_SPAWN_HEIGHT = 0.2f;
    private const float ARMY_SPAWN_HEIGHT = 0.5f;
    private const int PRODUCTION_DISTANCE = 8;
    private const int ARMY_DISTANCE = 7;
} 