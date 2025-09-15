using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using TMPro;
using Hexamap;
using UnityEngine.EventSystems;
using FischlWorks_FogWar;
using Cysharp.Threading.Tasks;
using Random = UnityEngine.Random;

[System.Serializable]
public struct MapSettings
{
    public float playerSpawnHeight;
    public float zombieSpawnHeight;
    public float mapOffset;
    public float noiseMultiplier;
    public int defaultMoveRange;
    public int zombieDetectionRange;
    public int sightRange;
    public int playerSightRange;
}

public class MapController : Singleton<MapController>
{
    [Header("Core Components")]
    [SerializeField] private HexamapController hexaMap;
    [SerializeField] private csFogWar fogOfWar;

    [Header("Transforms")]
    [SerializeField] private Transform zombiesTransform;
    [SerializeField] private Transform mapTransform;
    [SerializeField] private Transform mapParentTransform;
    [SerializeField] private Transform objectsTransform;
    [SerializeField] private GameObject arrowPrefab;

    [Header("Prefabs")]
    [SerializeField] private MapPrefabSO mapPrefab;

    [Header("Map Settings")]
    [SerializeField] private MapSettings mapSettings = new MapSettings
    {
        playerSpawnHeight = 0.7f,
        zombieSpawnHeight = 0.6f,
        mapOffset = 200f,
        noiseMultiplier = 2f,
        defaultMoveRange = 4,
        zombieDetectionRange = 2,
        sightRange = 5,
        playerSightRange = 2
    };

    [Header("Game Managers")]
    [SerializeField] private ZombieManager zombieManager;
    [SerializeField] private DroneManager droneManager;
    [SerializeField] private StructureManager structureManager;
    
    [Header("Sub Managers")]
    [SerializeField] private MapRenderingManager renderingManager;
    [SerializeField] private MapTileManager tileManager;
    [SerializeField] private MapPathfindingManager pathfindingManager;
    [SerializeField] private MapVisibilityManager visibilityManager;

    public static MapController Instance { get; private set; }
    public Player Player { get; private set; }
    public bool LoadingComplete => isLoadingComplete;
    public TileController TargetPointTile => targetTileController;
    
    public ZombieManager ZombieManager => zombieManager;
    public DroneManager DroneManager => droneManager;
    public StructureManager StructureManager => structureManager;

    private MapData mapData;
    private TileController targetTileController;
    private bool isLoadingComplete;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            InitializeSubManagers();
        }
        else
        {
            Destroy(gameObject);
        }
    }
    
    private void InitializeSubManagers()
    {
        if (renderingManager == null)
        {
            renderingManager = gameObject.AddComponent<MapRenderingManager>();
        }
        
        if (tileManager == null)
        {
            tileManager = gameObject.AddComponent<MapTileManager>();
        }
        
        if (pathfindingManager == null)
        {
            pathfindingManager = gameObject.AddComponent<MapPathfindingManager>();
        }
        
        if (visibilityManager == null)
        {
            visibilityManager = gameObject.AddComponent<MapVisibilityManager>();
        }
        
        renderingManager.Initialize(objectsTransform, this);
        tileManager.Initialize(hexaMap, this);
        pathfindingManager.Initialize(this, tileManager, arrowPrefab, mapSettings);
        visibilityManager.Initialize(this, tileManager, renderingManager, mapSettings);
    }

    private void Start()
    {
        var mapManager = App.instance.GetMapManager();
        if (mapManager != null)
        {
            mapManager.GetAdditiveSceneObjectsCoroutine();
        }
    }

    public async UniTask GenerateMapAsync()
    {
        renderingManager.ClearRenderingCache();
        
        hexaMap.Destroy();

        var timeBefore = DateTime.Now;

        hexaMap.Generate();

        double timeSpent = (DateTime.Now - timeBefore).TotalSeconds;

        hexaMap.Draw();

        FastNoise _fastNoise = new FastNoise();
        _fastNoise.SetFrequency(0.1f);
        _fastNoise.SetNoiseType(FastNoise.NoiseType.Perlin);
        _fastNoise.SetSeed(hexaMap.Map.Seed);

        await ApplyNoiseToTilesAsync(hexaMap.Map.Tiles, _fastNoise);

        mapParentTransform.position = Vector3.forward * mapSettings.mapOffset;

        GenerateMapObjects();

        isLoadingComplete = true;
    }
    
    public void GenerateMap()
    {
        GenerateMapAsync().Forget();
    }

    public void RegenerateMap()
    {
        Destroy(Player);
        zombieManager?.ClearAllZombies();
        GenerateMap();
    }

    public void SpawnTutorialZombie()
    {
        zombieManager?.SpawnTutorialZombie();
    }

    public void SpawnStructureZombies(List<TileBase> tiles)
    {
        zombieManager?.SpawnStructureZombies(tiles);
    }

    public void DefaultMouseOverState(TileController tileController)
    {
        tileManager.DefaultMouseOverState(tileController);
    }

    public void ExplorerPathFinder(TileController tileController, int num = 3)
    {
        tileManager.ExplorerPathFinder(tileController, num);
    }

    public void TilePathFinderSurroundings(TileController tileController)
    {
        tileManager.TilePathFinderSurroundings(tileController);
    }

    public void AddSelectedTilesList(TileController tileController)
    {
        tileManager.AddSelectedTilesList(tileController);
    }

    public void AddToDroneSelectedTiles(TileController tileController)
    {
        tileManager.AddToDroneSelectedTiles(tileController);
    }

    public List<TileController> GetDroneSelectedTiles()
    {
        return tileManager.GetDroneSelectedTiles();
    }

    public void DisturbtorPathFinder(TileController tileController)
    {
        tileManager.DisturbtorPathFinder(tileController);
    }

    public bool PlayerCanMoveCheck()
    {
        return pathfindingManager.PlayerCanMoveCheck();
    }

    public bool SelectPlayerMovePoint(TileController tileController)
    {
        return pathfindingManager.SelectPlayerMovePoint(tileController);
    }

    public void SelectTileForDisturbtor(TileController tileController)
    {
        pathfindingManager.SelectTileForDisturbtor(tileController);
    }

    public void SelectTileForExplorer(TileController tileController)
    {
        pathfindingManager.SelectTileForExplorer(tileController);
    }

    public void SavePlayerMovePath(TileController tileController)
    {
        pathfindingManager.SavePlayerMovePath(tileController);
    }

    public void DeletePlayerMovePath()
    {
        pathfindingManager.DeletePlayerMovePath();
    }

    public bool IsMovePathSaved()
    {
        return pathfindingManager.IsMovePathSaved();
    }
    
    public void SetTargetTileController(TileController tileController)
    {
        targetTileController = tileController;
    }

    public TileController TileToTileController(Tile tile)
    {
        return tileManager.TileToTileController(tile);
    }

    public void PreparingDistrubtor(bool set)
    {
        droneManager?.PreparingDistrubtor(set);
    }

    public void PreparingExplorer(bool set)
    {
        droneManager?.PreparingExplorer(set);
    }

    public void InstallDistrubtor(TileController tileController, CompassPoint direction)
    {
        pathfindingManager.InstallDistrubtor(tileController, direction);
    }

    public void InstallExplorer(TileController tileController)
    {
        pathfindingManager.InstallExplorer(tileController);
    }

    public async UniTask NextDayAsync()
    {
        await HandlePlayerTurnAsync();
        HandleDrones();
        await HandleZombiesAsync();
        HandleEndOfDay();
    }

    public void CheckSumZombies()
    {
        zombieManager?.CheckSumZombies();
    }

    public void SelectTargetBorder(TileController tileController)
    {
        tileManager.SelectTargetBorder(tileController);
    }

    public void DeselecTargetBorder(TileController tileController)
    {
        tileManager.DeselecTargetBorder(tileController);
    }

    public bool CheckPlayerInStructureTile(TileController tileController)
    {
        var structure = tileController.gameObject.GetComponent<TileBase>().Structure;

        if (structure != null)
        {
            if (tileController.gameObject.GetComponent<TileBase>().Structure.IsAccessible)
            {
                return true;
            }
            else
            {
                return false;
            }
        }
        else
            return false;
    }

    public void DeselectAllBorderTiles()
    {
        tileManager.DeselectAllBorderTiles();
    }

    public void DeselectAllTargetTiles()
    {
        tileManager.DeselectAllTargetTiles();
    }

    public Tile GetTileFromCoords(Coords coords)
    {
        return tileManager.GetTileFromCoords(coords);
    }

    public List<Tile> GetTilesInRange(Tile tile, int num)
    {
        return tileManager.GetTilesInRange(tile, num);
    }

    public bool CalculateDistanceToPlayer(Tile tile, int range)
    {
        return pathfindingManager.CalculateDistanceToPlayer(tile, range);
    }

    public Distrubtor CalculateDistanceToDistrubtor(Tile tile, int range)
    {
        return pathfindingManager.CalculateDistanceToDistrubtor(tile, range);
    }

    public bool CheckPlayersView(TileController tileController)
    {
        return pathfindingManager.CheckPlayersView(tileController);
    }

    public bool CheckZombies()
    {
        return zombieManager?.CheckZombiesNearPlayer(Player, mapSettings.zombieDetectionRange) ?? false;
    }

    public void GenerateTower()
    {
        structureManager?.GenerateTower();
    }

    public void Generate7TileStructure(Coords _coords)
    {
        structureManager?.Generate7TileStructure(_coords);
    }

    public void Generate3TileStructure(Coords _coords)
    {
        structureManager?.Generate3TileStructure(_coords);
    }

    public void SpawnSpecialItemRandomTile(List<TileBase> tileBases)
    {
        structureManager?.SpawnSpecialItemRandomTile(tileBases);
    }

    public StructureBase SensingStructure()
    {
        return structureManager?.SensingStructure(Player);
    }

    public bool SensingSignalTower()
    {
        return structureManager?.SensingSignalTower(Player) ?? false;
    }

    public bool SensingProductionStructure()
    {
        return structureManager?.SensingProductionStructure(Player) ?? false;
    }

    public StructureType GetStructureType(StructureBase structure)
    {
        return structureManager?.GetStructureType(structure) ?? StructureType.Tower;
    }

    public List<int> RandomTileSelect(EObjectSpawnType type, int choiceNum = 1)
    {
        var tiles = GetAllTiles();
        return RandomTileSelect(tiles, type, choiceNum);
    }

    public List<int> RandomTileSelect(List<Tile> tiles, EObjectSpawnType type, int choiceNum = 1)
    {
        List<int> selectTileNumber = new List<int>();

        if (tiles == null || tiles.Count == 0)
        {
            selectTileNumber.Add(5);
            return selectTileNumber;
        }

        while (selectTileNumber.Count != choiceNum)
        {
            int randomInt = Random.Range(0, tiles.Count);

            if (ConditionalBranch(type, tiles[randomInt]))
            {
                if (selectTileNumber.Contains(randomInt) == false)
                {
                    selectTileNumber.Add(randomInt);
                    structureManager?.PreemptiveTiles.Add(tiles[randomInt]);
                }
            }
        }

        return selectTileNumber;
    }

    public bool CheckTileType(Tile tile, string type)
    {
        return tile.Landform.GetType().Name == type;
    }

    public void OcclusionCheck(Tile _targetTile)
    {
        visibilityManager.OcclusionCheck(_targetTile);
    }
    

    public async void SightCheckInit()
    {
        await UniTask.Delay(100);
        OcclusionCheck(GetTileFromCoords(new Coords(0, 0)));
    }

    public List<Tile> GetPlayerSightTiles()
    {
        return visibilityManager.GetPlayerSightTiles();
    }
    
    public List<Tile> GetPlayerSightTilesForParticles()
    {
        return visibilityManager.GetPlayerSightTilesForParticles();
    }

    public List<Tile> GetSightTiles(Tile tile)
    {
        return visibilityManager.GetSightTiles(tile);
    }

    public void InputMapData(MapData _mapData)
    {
        mapData = _mapData;
    }

    public void RemoveDistrubtor(Distrubtor _distrubtor)
    {
        droneManager?.RemoveDistrubtor(_distrubtor);
    }

    public void RemoveExplorer(Explorer _explorer)
    {
        droneManager?.RemoveExplorer(_explorer);
    }

    public void InvocationExplorers()
    {
        droneManager?.InvocationExplorers();
    }

    public bool LandformCheck(TileController tileController)
    {
        if (CheckTileType(tileController.Model, "LandformPlain") ||
            CheckTileType(tileController.Model, "LandformRocks"))
        {
            return true;
        }

        return false;
    }

    public void MovePointerOn(Vector3 _pos)
    {
        pathfindingManager.MovePointerOn(_pos);
    }
    
    public void MovePointerOff()
    {
        pathfindingManager.MovePointerOff();
    }
    
    public void OnlyMovePointerOff()
    {
        pathfindingManager.OnlyMovePointerOff();
    }

    private void GenerateMapObjects()
    {
        LoadGameData();
        SpawnPlayer();
        InitializeManagers();
        GenerateStructures();
        if (zombieManager != null)
            zombieManager.SpawnZombies(mapData.zombieCount, mapData);
        InitializeFogOfWar();
        RandomTileResource(mapData.resourcePercent);
    }

    private void LoadGameData()
    {
        App.instance.GetDataManager().gameData.TryGetValue("Data_MinCount_ZombieObject", out GameData min);
        App.instance.GetDataManager().gameData.TryGetValue("Data_MaxCount_ZombieObject", out GameData max);
    }

    private void GenerateStructures()
    {
        if (structureManager != null)
        {
            structureManager.GenerateTower();
            structureManager.Generate3TileStructure(new Coords(0, 0));
            structureManager.Generate7TileStructure(new Coords(0, 0));
            
            InvalidateStructureCache();
        }
    }

    private void InitializeFogOfWar()
    {
        csFogWar.instance.InitializeMapControllerObjects(Player.gameObject, mapData.fogSightRange);
        DeselectAllBorderTiles();
    }

    private void InitializeManagers()
    {
        if (zombieManager == null)
            zombieManager = GetComponent<ZombieManager>();
        if (zombieManager == null)
            zombieManager = GetComponentInChildren<ZombieManager>();
            
        if (droneManager == null)
            droneManager = GetComponent<DroneManager>();
        if (droneManager == null)
            droneManager = GetComponentInChildren<DroneManager>();
            
        if (structureManager == null)
            structureManager = GetComponent<StructureManager>();
        if (structureManager == null)
            structureManager = GetComponentInChildren<StructureManager>();

        if (zombieManager == null)
        {
            zombieManager = gameObject.AddComponent<ZombieManager>();
        }
        
        if (droneManager == null)
        {
            droneManager = gameObject.AddComponent<DroneManager>();
        }
        
        if (structureManager == null)
        {
            structureManager = gameObject.AddComponent<StructureManager>();
        }

        if (zombieManager != null)
            zombieManager.Initialize(this, zombiesTransform, mapPrefab);
        
        if (droneManager != null)
        {
            if (Player == null)
            {
                return;
            }
            else
            {
                droneManager.Initialize(this, Player, mapTransform, mapPrefab);
            }
        }
        
        if (structureManager != null)
            structureManager.Initialize(this, objectsTransform, mapPrefab);
    }

    private async void RandomTileResource(float _percent)
    {
        List<TileBase> tileBaseList = GetAllTiles()
            .Select(x => ((GameObject)x.GameEntity).GetComponent<TileBase>())
            .ToList();

        float randomTileCount = tileBaseList.Count - (tileBaseList.Count * (_percent * 0.01f));

        for (int i = 0; i < randomTileCount; ++i)
        {
            int randNum = Random.Range(0, tileBaseList.Count);
            tileBaseList.RemoveAt(randNum);
        }

        await SpawnResourcesInBatchesAsync(tileBaseList);

        OcclusionCheck(Player.TileController.Model);
    }
    
    private async UniTask SpawnResourcesInBatchesAsync(List<TileBase> tileBaseList)
    {
        int batchSize = 15;
        int processedCount = 0;
        
        for (int i = 0; i < tileBaseList.Count; i++)
        {
            TileBase tile = tileBaseList[i];
            tile.SpawnRandomResource();
            
            processedCount++;
            
            if (processedCount % batchSize == 0)
            {
                await UniTask.Yield();
            }
        }
    }

    public List<Tile> GetAllTiles()
    {
        return tileManager.GetAllTiles();
    }

    private void InvalidateTilesCache()
    {
        tileManager.InvalidateTilesCache();
    }
    
    private void InvalidateStructureCache()
    {
        renderingManager.InvalidateStructureCache();
    }
    
    private void InvalidateAllRenderingCache()
    {
        renderingManager.ClearRenderingCache();
        tileManager.InvalidateTilesCache();
        visibilityManager.InvalidatePlayerSightCache();
    }
    
    private async UniTask ApplyNoiseToTilesAsync(IReadOnlyList<Tile> tiles, FastNoise fastNoise)
    {
        int batchSize = 20;
        int processedCount = 0;
        
        foreach (Tile tile in tiles)
        {
            var noiseY = fastNoise.GetValue(tile.Coords.X, tile.Coords.Y);
            (tile.GameEntity as GameObject).transform.position += new Vector3(0, noiseY * mapSettings.noiseMultiplier, 0);
            
            processedCount++;
            
            if (processedCount % batchSize == 0)
            {
                await UniTask.Yield();
            }
        }
    }

    private void SpawnPlayer()
    {
        Vector3 spawnPos = TileToTileController(hexaMap.Map.GetTileFromCoords(new Coords(0, 0))).transform.position;
        spawnPos.y += mapSettings.playerSpawnHeight;

        var playerObject = Instantiate(mapPrefab.items[(int)EMabPrefab.Player].prefab, spawnPos,
            Quaternion.Euler(0, -90, 0));
        Player = playerObject.GetComponent<Player>();
        Player.transform.parent = mapParentTransform;
        Player.InputDefaultData(mapData.playerMovementPoint, mapData.durability);

        Player.UpdateCurrentTile(TileToTileController(hexaMap.Map.GetTileFromCoords(new Coords(0, 0))));
        targetTileController = Player.TileController;
        FloatingAnimationAsync().Forget();

        if (structureManager != null)
        {
            structureManager.PreemptiveTiles.Add(Player.TileController.Model);

            //Player.TileEffectCheck();

            foreach (var item in GetTilesInRange(Player.TileController.Model, mapSettings.defaultMoveRange))
            {
                structureManager.PreemptiveTiles.Add(item);
            }
        }
    }

    private async UniTask FloatingAnimationAsync()
    {
        await UniTask.WaitUntil(() => Player != null);
        Player.StartFloatingAnimation();
    }

    private async UniTask HandlePlayerTurnAsync()
    {
        Player.ChangeClockBuffDuration();
        
        if (Player.MovePath != null)
        {
            await Player.ActionDecisionAsync(targetTileController);
        }
        else
        {
            DeselectAllBorderTiles();
        }
    }

    private void HandleDrones()
    {
        droneManager.HandleDrones();
    }

    private async UniTask HandleZombiesAsync()
    {
        await zombieManager.HandleZombiesAsync();
    }

    private void HandleEndOfDay()
    {
        Player.SetHealth(true);
        Player.TileEffectCheck();
        
        visibilityManager.InvalidatePlayerSightCache();
        OcclusionCheck(Player.TileController.Model);
    }


    private bool ConditionalBranch(EObjectSpawnType type, Tile tile)
    {
        if (LandformCheck(TileToTileController(tile)) == false)
        {
            return false;
        }

        switch (type)
        {
            case EObjectSpawnType.ExcludePlayer:
                if (Player.TileController.Model != tile)
                    return true;
                else
                    return false;

            case EObjectSpawnType.IncludePlayer:
                return true;

            case EObjectSpawnType.ExcludeEntites:
                if (structureManager?.PreemptiveTiles.Contains(tile) == false)
                    return true;
                else
                    return false;

            case EObjectSpawnType.IncludeEntites:
                if (Player.TileController.Model != tile)
                    return true;
                else
                    return false;

            default:
                break;
        }

        return false;
    }
}