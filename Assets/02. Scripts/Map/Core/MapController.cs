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
    [Header("Components")]
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

    [Header("Settings")]
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

    [Header("Managers")]
    [SerializeField] private ZombieManager zombieManager;
    [SerializeField] private DroneManager droneManager;
    [SerializeField] private StructureManager structureManager;

    public Player Player { get; private set; }
    public bool LoadingComplete => isLoadingComplete;
    public TileController TargetPointTile => targetTileController;

    private List<TileController> selectedTiles = new List<TileController>();
    private List<TileController> droneSelectedTiles = new List<TileController>();
    private List<TileController> pathTiles = new List<TileController>();
    private List<Tile> sightTiles = new List<Tile>();
    private MapData mapData;
    private TileController targetTileController;
    private bool isLoadingComplete;
    private List<Tile> _cachedAllTiles;
    private bool _tilesCacheDirty = true;
    private GameObject arrow;

    private Dictionary<GameObject, Renderer> _rendererCache = new Dictionary<GameObject, Renderer>();
    private Dictionary<GameObject, bool> _lastVisibilityState = new Dictionary<GameObject, bool>();
    private HashSet<GameObject> _visibleObjects = new HashSet<GameObject>();
    private HashSet<GameObject> _invisibleObjects = new HashSet<GameObject>();
    
    private List<StructureObject> _cachedStructureObjects;
    private List<TileController> _cachedTileControllers;
    private HashSet<Tile> _cachedVisibleTiles;
    private HashSet<Tile> _cachedPlayerSightTiles;
    private bool _playerSightCacheDirty = true;
    
    private List<GameObject> _tempShowList;
    private List<GameObject> _tempHideList;

    private void Start()
    {
        App.instance.GetMapManager().GetAdditiveSceneObjectsCoroutine();
    }

    public async UniTask GenerateMapAsync()
    {
        InvalidateAllRenderingCache();
        
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
        if (LandformCheck(tileController) == false)
        {
            SelectBorder(tileController, ETileState.Unable);
        }
        else if (tileController != null && !selectedTiles.Contains(tileController))
        {
            SelectBorder(tileController, ETileState.Unable);
        }
    }

    public void ExplorerPathFinder(TileController tileController, int num = 3)
    {
        int moveRange = 0;
        if (tileController.Model != Player.TileController.Model)
        {
            foreach (Coords coords in AStar.FindPath(Player.TileController.Model.Coords, tileController.Model.Coords))
            {
                if (moveRange == num)
                    break;

                var tile = TileToTileController(GetTileFromCoords(coords));

                if (LandformCheck(tile) == false)
                    continue;

                SelectBorder(tile, ETileState.None);
                selectedTiles.Add(tile);
                moveRange++;
            }

            if (moveRange != num && tileController.gameObject.GetComponent<TileBase>().Structure?.IsAccessible == false)
                SelectBorder(tileController, ETileState.Unable);
            else
                SelectBorder(tileController, ETileState.Moveable);
        }
        else
        {
            SelectBorder(tileController, ETileState.Unable);
        }
    }

    public void TilePathFinderSurroundings(TileController tileController)
    {
        var neighborTiles = hexaMap.Map.GetTilesInRange(Player.TileController.Model, Player.MoveRange);

        var neighborController = neighborTiles
            .Select(x => ((GameObject)x.GameEntity).GetComponent<TileController>()).ToList();

        for (var index = 0; index < neighborController.Count; index++)
        {
            var value = neighborController[index];

            if (LandformCheck(value) == false)
                continue;

            selectedTiles.Add(value);
            SelectBorder(value, ETileState.None);
        }

        if (tileController.gameObject.GetComponent<TileBase>().Structure?.IsAccessible == false
            || LandformCheck(tileController) == false)
        {
            SelectBorder(tileController, ETileState.Unable);
        }
        else if (neighborTiles.Contains(tileController.Model) == false)
        {
            SelectBorder(tileController, ETileState.Unable);
        }
        else if (tileController.gameObject.GetComponent<TileBase>().CurZombies != null)
        {
            SelectBorder(tileController, ETileState.Unable);
        }
        else if (neighborTiles.Contains(tileController.Model))
        {
            SelectBorder(tileController, ETileState.Moveable);
        }
    }

    public void AddSelectedTilesList(TileController tileController)
    {
        selectedTiles.Add(tileController);
    }

    public void AddToDroneSelectedTiles(TileController tileController)
    {
        droneSelectedTiles.Add(tileController);
    }

    public List<TileController> GetDroneSelectedTiles()
    {
        return droneSelectedTiles;
    }

    public void DisturbtorPathFinder(TileController tileController)
    {
        if (droneSelectedTiles.Contains(tileController))
        {
            var currentDistrubtor = droneManager?.CurrentDistrubtor;
            if (currentDistrubtor != null)
            {
                currentDistrubtor.transform.position =
                    ((GameObject)tileController.Model.GameEntity).transform.position + Vector3.up;

                currentDistrubtor.GetComponent<Distrubtor>().DirectionObjectOff();

                if (LandformCheck(tileController))
                    SelectBorder(tileController, ETileState.Moveable);

                foreach (var item in Player.TileController.Model.Neighbours.Where(
                             item => item.Value == tileController.Model))
                {
                    currentDistrubtor.GetComponent<Distrubtor>().GetDirectionObject(item.Key).SetActive(true);
                }
            }
        }
        else
        {
            SelectBorder(tileController, ETileState.Unable);
        }
    }

    public bool PlayerCanMoveCheck()
    {
        return Player.MoveRange != 0;
    }

    public bool SelectPlayerMovePoint(TileController tileController)
    {
        if (tileController.GetComponent<Borders>().GetEtileState() == ETileState.Moveable
            && Player.TileController.Model != tileController.Model
            && LandformCheck(tileController))
        {
            SavePlayerMovePath(tileController);
            return true;
        }
        else
            return false;
    }

    public void SelectTileForDisturbtor(TileController tileController)
    {
        if (LandformCheck(tileController) == false)
            return;

        if (tileController.GetComponent<Borders>().GetEtileState() == ETileState.Moveable
            && Player.TileController.Model != tileController.Model)
        {
            foreach (var item in Player.TileController.Model.Neighbours.Where(
                         item => item.Value == tileController.Model))
            {
                InstallDistrubtor(tileController, item.Key);
            }
        }
    }

    public void SelectTileForExplorer(TileController tileController)
    {
        if (tileController.GetComponent<Borders>().GetEtileState() == ETileState.Moveable
            && Player.TileController.Model != tileController.Model)
        {
            InstallExplorer(tileController);
        }
    }

    public void SavePlayerMovePath(TileController tileController)
    {
        targetTileController = tileController;

        Player.UpdateMovePath(AStar.FindPath(Player.TileController.Model.Coords, tileController.Model.Coords));

        DeselectAllBorderTiles();
        //isPlayerSelected = false;
    }

    public void DeletePlayerMovePath()
    {
        Player.UpdateMovePath(null);
        DeselectAllBorderTiles();
    }

    public bool IsMovePathSaved()
    {
        return Player.MovePath != null;
    }

    public TileController TileToTileController(Tile tile)
    {
        return ((GameObject)tile.GameEntity).GetComponent<TileController>();
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
        droneManager?.InstallDistrubtor(tileController, direction);
    }

    public void InstallExplorer(TileController tileController)
    {
        droneManager?.InstallExplorer(tileController);
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
        tileController.GetComponent<Borders>().GetDisturbanceBorder().SetActive(true);
        droneSelectedTiles.Add(tileController);
    }

    public void DeselecTargetBorder(TileController tileController)
    {
        tileController.GetComponent<Borders>().OffTargetBorder();

        if (droneSelectedTiles.Contains(tileController))
            droneSelectedTiles.Remove(tileController);
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
        if (selectedTiles?.Count > 0)
        {
            ClearTiles(selectedTiles);
        }

        if (pathTiles?.Count > 0)
        {
            ClearTiles(pathTiles);
        }
    }

    public void DeselectAllTargetTiles()
    {
        if (droneSelectedTiles?.Count > 0)
        {
            for (int i = 0; i < droneSelectedTiles.Count; i++)
            {
                TileController tile = droneSelectedTiles[i];
                if (tile != null)
                {
                    DeselecTargetBorder(tile);
                }
            }
            droneSelectedTiles.Clear();
        }
    }

    public Tile GetTileFromCoords(Coords coords)
    {
        return hexaMap.Map.GetTileFromCoords(coords);
    }

    public List<Tile> GetTilesInRange(Tile tile, int num)
    {
        return hexaMap.Map.GetTilesInRange(tile, num);
    }

    public bool CalculateDistanceToPlayer(Tile tile, int range)
    {
        var searchTiles = hexaMap.Map.GetTilesInRange(tile, range);

        return searchTiles.Exists(x => x == Player.TileController.Model);
    }

    public Distrubtor CalculateDistanceToDistrubtor(Tile tile, int range)
    {
        return droneManager?.CalculateDistanceToDistrubtor(tile, range);
    }

    public bool CheckPlayersView(TileController tileController)
    {
        if (tileController == null || Player?.TileController == null)
            return false;

        if (Player.TileController == tileController)
            return true;

        var getTiles = GetTilesInRange(Player.TileController.Model, 3);
        return getTiles.Contains(tileController.Model);
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
        sightTiles = GetTilesInRange(_targetTile, mapSettings.sightRange);
        sightTiles.Add(_targetTile);

        _playerSightCacheDirty = true;
        
        OptimizeRenderingAsync().Forget();
        
        UpdateAllTileVisibilityAsync().Forget();
    }
    
    private async UniTask OptimizeRenderingAsync()
    {
        OptimizeStructureRendering();
        await UniTask.Yield();
        
        OptimizeTileRendering();
        await UniTask.Yield();
        
        await UpdateParticleLODAsync();
        await UniTask.Yield();
        
        CleanupVisibilityCache();
    }
    
    private async UniTask UpdateAllTileVisibilityAsync()
    {
        if (Player?.TileController?.Model == null) return;
        
        var playerSightTiles = GetPlayerSightTiles();
        var sightHashSet = new HashSet<Tile>(playerSightTiles);
        
        if (_cachedTileControllers == null)
        {
            var allTiles = GetAllTiles();
            _cachedTileControllers = allTiles
                .Select(x => ((GameObject)x.GameEntity).GetComponent<TileController>())
                .Where(x => x != null)
                .ToList();
        }
        
        var batchSize = 50;
        
        for (int i = 0; i < _cachedTileControllers.Count; i += batchSize)
        {
            int endIndex = Mathf.Min(i + batchSize, _cachedTileControllers.Count);
            
            for (int j = i; j < endIndex; j++)
            {
                var tileController = _cachedTileControllers[j];
                if (tileController != null)
                {
                    var tileBase = tileController.GetComponent<TileBase>();
                    if (tileBase != null)
                    {
                        bool inSight = sightHashSet.Contains(tileController.Model);
                        tileBase.UpdateVisibilityFromController(inSight);
                    }
                }
            }
            
            if (i + batchSize < _cachedTileControllers.Count)
                await UniTask.Yield();
        }
    }

    private void OptimizeStructureRendering()
    {
        if (_cachedStructureObjects == null)
        {
            _cachedStructureObjects = objectsTransform.GetComponentsInChildren<StructureObject>(true).ToList();
        }
        
        List<StructureObject> structureObjects = _cachedStructureObjects;

        for (int i = 0; i < structureObjects.Count; i++)
        {
            StructureObject item = structureObjects[i];
            GameObject obj = item.gameObject;
            
            bool shouldBeVisible = sightTiles.Contains(item.CurTile);
            
            if (_lastVisibilityState.TryGetValue(obj, out bool lastState))
            {
                if (lastState != shouldBeVisible)
                {
                    SetObjectVisibility(obj, shouldBeVisible);
                    _lastVisibilityState[obj] = shouldBeVisible;
                }
            }
            else
            {
                SetObjectVisibility(obj, shouldBeVisible);
                _lastVisibilityState[obj] = shouldBeVisible;
            }
        }
    }


    private void OptimizeTileRendering()
    {
        if (_cachedTileControllers == null)
        {
            var allTiles = GetAllTiles();
            _cachedTileControllers = allTiles
                .Select(x => ((GameObject)x.GameEntity).GetComponent<TileController>())
                .Where(x => x != null)
                .ToList();
        }
        
        if (_cachedVisibleTiles == null)
        {
            _cachedVisibleTiles = new HashSet<Tile>();
        }
        else
        {
            _cachedVisibleTiles.Clear();
        }
        
        foreach (var tile in sightTiles)
        {
            _cachedVisibleTiles.Add(tile);
        }
        
        if (_tempShowList == null)
            _tempShowList = new List<GameObject>();
        else
            _tempShowList.Clear();
            
        if (_tempHideList == null)
            _tempHideList = new List<GameObject>();
        else
            _tempHideList.Clear();
        
        for (int i = 0; i < _cachedTileControllers.Count; i++)
        {
            TileController tileController = _cachedTileControllers[i];
            if (tileController == null) continue;
            
            GameObject tileObj = tileController.gameObject;
            bool shouldBeVisible = _cachedVisibleTiles.Contains(tileController.Model);
            
            if (_lastVisibilityState.TryGetValue(tileObj, out bool lastState))
            {
                if (lastState != shouldBeVisible)
                {
                    if (shouldBeVisible)
                        _tempShowList.Add(tileObj);
                    else
                        _tempHideList.Add(tileObj);
                    
                    _lastVisibilityState[tileObj] = shouldBeVisible;
                }
            }
            else
            {
                if (shouldBeVisible)
                    _tempShowList.Add(tileObj);
                else
                    _tempHideList.Add(tileObj);
                
                _lastVisibilityState[tileObj] = shouldBeVisible;
            }
        }
        
        BatchSetVisibility(_tempShowList, true);
        BatchSetVisibility(_tempHideList, false);
    }
    
    private void SetObjectVisibility(GameObject obj, bool visible)
    {
        if (obj == null) return;
        
        if (!_rendererCache.TryGetValue(obj, out Renderer renderer))
        {
            renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                _rendererCache[obj] = renderer;
        }
        
        if (renderer != null)
        {
            renderer.enabled = visible;
            
            Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer != renderer)
                    childRenderer.enabled = visible;
            }
        }
        else
        {
            obj.SetActive(visible);
        }
        
        if (visible)
            _visibleObjects.Add(obj);
        else
            _invisibleObjects.Add(obj);
    }

    private void BatchSetVisibility(List<GameObject> objects, bool visible)
    {
        if (objects.Count == 0) return;
        
        var rendererGroups = new Dictionary<Renderer, List<GameObject>>();
        
        foreach (var obj in objects)
        {
            if (!_rendererCache.TryGetValue(obj, out Renderer renderer))
            {
                renderer = obj.GetComponent<Renderer>();
                if (renderer != null)
                    _rendererCache[obj] = renderer;
            }
            
            if (renderer != null)
            {
                if (!rendererGroups.ContainsKey(renderer))
                    rendererGroups[renderer] = new List<GameObject>();
                rendererGroups[renderer].Add(obj);
            }
        }
        
        foreach (var group in rendererGroups)
        {
            Renderer renderer = group.Key;
            renderer.enabled = visible;
            
            Renderer[] childRenderers = renderer.GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer != renderer)
                    childRenderer.enabled = visible;
            }
        }
        
        foreach (var obj in objects)
        {
            if (!_rendererCache.ContainsKey(obj))
            {
                obj.SetActive(visible);
            }
        }
    }

    private void CleanupVisibilityCache()
    {
        var keysToRemove = new List<GameObject>();
        
        foreach (var kvp in _lastVisibilityState)
        {
            if (kvp.Key == null)
                keysToRemove.Add(kvp.Key);
        }
        
        foreach (var key in keysToRemove)
        {
            _lastVisibilityState.Remove(key);
            _rendererCache.Remove(key);
            _visibleObjects.Remove(key);
            _invisibleObjects.Remove(key);
        }
        
        if (_lastVisibilityState.Count > 1000)
        {
            var oldestKeys = _lastVisibilityState.Keys.Take(100).ToList();
            foreach (var key in oldestKeys)
            {
                _lastVisibilityState.Remove(key);
                _rendererCache.Remove(key);
            }
        }
    }

    public void ClearRenderingCache()
    {
        _rendererCache.Clear();
        _lastVisibilityState.Clear();
        _visibleObjects.Clear();
        _invisibleObjects.Clear();
    }
    
    private async UniTask UpdateParticleLODAsync()
    {
        var mapManager = App.instance.GetMapManager();
        if (mapManager != null)
        {
            await mapManager.UpdateAllParticleLODAsync();
        }
    }

    public async void SightCheckInit()
    {
        await UniTask.Delay(100);
        OcclusionCheck(GetTileFromCoords(new Coords(0, 0)));
    }

    public List<Tile> GetPlayerSightTiles()
    {
        if (_playerSightCacheDirty || _cachedPlayerSightTiles == null)
        {
            if (_cachedPlayerSightTiles == null)
                _cachedPlayerSightTiles = new HashSet<Tile>();
            else
                _cachedPlayerSightTiles.Clear();
                
            var tiles = GetTilesInRange(Player.TileController.Model, mapSettings.playerSightRange);
            foreach (var tile in tiles)
            {
                _cachedPlayerSightTiles.Add(tile);
            }
            _playerSightCacheDirty = false;
        }
        
        return _cachedPlayerSightTiles.ToList();
    }
    
    public List<Tile> GetPlayerSightTilesForParticles()
    {
        var list = GetTilesInRange(Player.TileController.Model, mapSettings.playerSightRange + 3);
        return list;
    }

    public List<Tile> GetSightTiles(Tile tile)
    {
        var list = GetTilesInRange(tile, mapSettings.playerSightRange);
        return list;
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
        if (arrow == null)
        {
            arrow = Instantiate(arrowPrefab, _pos, Quaternion.identity);
        }
        
        _pos.y += mapSettings.playerSpawnHeight;
        arrow.transform.position = _pos;
        
        arrow.SetActive(true);
        App.instance.GetSoundManager().PlaySFX("SFX_Map_Select_Complete");
    }
    
    public void MovePointerOff()
    {
        if (arrow == null)
        {
            arrow = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity);
        }
        
        if (arrow.activeInHierarchy)
        {
            arrow.SetActive(false);
            App.instance.GetSoundManager().PlaySFX("SFX_Map_Select_Cancel");
        }
    }
    
    public void OnlyMovePointerOff()
    {
        if (arrow == null)
        {
            arrow = Instantiate(arrowPrefab, Vector3.zero, Quaternion.identity);
        }
        
        if (arrow.activeInHierarchy)
        {
            arrow.SetActive(false);
        }
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
        if (_tilesCacheDirty || _cachedAllTiles == null)
        {
            _cachedAllTiles = hexaMap.Map.Tiles.Where(x => ((GameObject)x.GameEntity).CompareTag("Tile")).ToList();
            _tilesCacheDirty = false;
        }
        return _cachedAllTiles;
    }

    private void InvalidateTilesCache()
    {
        _tilesCacheDirty = true;
    }
    
    private void InvalidateStructureCache()
    {
        _cachedStructureObjects = null;
    }
    
    private void InvalidateAllRenderingCache()
    {
        _cachedStructureObjects = null;
        _cachedTileControllers = null;
        _cachedVisibleTiles = null;
        _cachedPlayerSightTiles = null;
        _tilesCacheDirty = true;
        _playerSightCacheDirty = true;
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
        
        _playerSightCacheDirty = true;
        OcclusionCheck(Player.TileController.Model);
    }

    private void SelectBorder(TileController tileController, ETileState state)
    {
        switch (state)
        {
            case ETileState.None:
                tileController.GetComponent<Borders>().GetNormalBorder().SetActive(true);
                break;
            case ETileState.Moveable:
                tileController.GetComponent<Borders>().GetAvailableBorder().SetActive(true);
                break;
            case ETileState.Unable:
                tileController.GetComponent<Borders>().GetUnAvailableBorder().SetActive(true);
                break;
            case ETileState.Target:

                break;
        }

        selectedTiles.Add(tileController);
    }

    private void DeselectNormalBorder(TileController tileController)
    {
        tileController.GetComponent<Borders>().OffNormalBorder();

        if (selectedTiles.Contains(tileController))
            selectedTiles.Remove(tileController);
    }

    private void ClearTiles(List<TileController> tiles)
    {
        for (int i = 0; i < tiles.Count; i++)
        {
            TileController tile = tiles[i];
            DeselectNormalBorder(tile);
        }

        tiles.Clear();
    }

    private GameObject GetTileBorder(TileController tileController, ETileState state)
    {
        switch (state)
        {
            case ETileState.None:
                return tileController.GetComponent<Borders>().GetNormalBorder();
            case ETileState.Moveable:
                return tileController.GetComponent<Borders>().GetAvailableBorder();
            case ETileState.Unable:
                return tileController.GetComponent<Borders>().GetUnAvailableBorder();
            case ETileState.Target:
                return tileController.GetComponent<Borders>().GetDisturbanceBorder();
        }

        return null;
    }

    private void SelectMetaLandform(TileController tile)
    {
        // Select metalandform of a tile
        var metaLandformTiles = tile
            .Model
            .Landform
            .MetaLandform
            .Tiles
            .Select(t => t.GameEntity)
            .Cast<GameObject>()
            .Select(g => g.GetComponent<TileController>())
            .ToList();

        var tileToUnselect = selectedTiles.Except(metaLandformTiles).ToList();
        var tileToSelect = metaLandformTiles.Except(selectedTiles).ToList();

        tileToSelect.ForEach(t => SelectBorder(t, ETileState.Unable));
        tileToUnselect.ForEach(t => DeselectNormalBorder(t));
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