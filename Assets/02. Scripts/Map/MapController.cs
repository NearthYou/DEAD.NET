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
    // SerializeField 변수들
    [Header("컴포넌트")] [Space(5f)] 
    [SerializeField] private HexamapController hexaMap;
    [SerializeField] private csFogWar fogOfWar;

    [Header("트랜스폼")] [Space(5f)] 
    [SerializeField] private Transform zombiesTransform;
    [SerializeField] private Transform mapTransform;
    [SerializeField] private Transform mapParentTransform;
    [SerializeField] private Transform objectsTransform;
    [SerializeField] private GameObject arrowPrefab;

    [Header("프리팹")] [Space(5f)] 
    [SerializeField] private MapPrefabSO mapPrefab;

    [Header("설정")] [Space(5f)] 
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

    [Header("매니저들")] [Space(5f)] 
    [SerializeField] private ZombieManager zombieManager;
    [SerializeField] private DroneManager droneManager;
    [SerializeField] private StructureManager structureManager;

    // Public 변수들
    public Player Player { get; private set; }
    public bool LoadingComplete => isLoadingComplete;
    public TileController TargetPointTile => targetTileController;

    // Private 변수들
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

    // 렌더링 최적화를 위한 캐시 변수들
    private Dictionary<GameObject, Renderer> _rendererCache = new Dictionary<GameObject, Renderer>();
    private Dictionary<GameObject, bool> _lastVisibilityState = new Dictionary<GameObject, bool>();
    private HashSet<GameObject> _visibleObjects = new HashSet<GameObject>();
    private HashSet<GameObject> _invisibleObjects = new HashSet<GameObject>();

    private void Start()
    {
        App.instance.GetMapManager().GetAdditiveSceneObjectsCoroutine();
    }

    public IEnumerator GenerateMap()
    {
        hexaMap.Destroy();

        var timeBefore = DateTime.Now;

        hexaMap.Generate();

        double timeSpent = (DateTime.Now - timeBefore).TotalSeconds;

        hexaMap.Draw();

        FastNoise _fastNoise = new FastNoise();
        _fastNoise.SetFrequency(0.1f);
        _fastNoise.SetNoiseType(FastNoise.NoiseType.Perlin);
        _fastNoise.SetSeed(hexaMap.Map.Seed);

        foreach (Tile tile in hexaMap.Map.Tiles)
        {
            var noiseY = _fastNoise.GetValue(tile.Coords.X, tile.Coords.Y);
            (tile.GameEntity as GameObject).transform.position += new Vector3(0, noiseY * mapSettings.noiseMultiplier, 0);
        }

        mapParentTransform.position = Vector3.forward * mapSettings.mapOffset;

        yield return StartCoroutine(GenerateMapObjects());

        isLoadingComplete = true;
    }

    public void RegenerateMap()
    {
        Destroy(Player);
        zombieManager?.ClearAllZombies();
        StartCoroutine(GenerateMap());
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
                Debug.Log("설치 시작");
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

    public IEnumerator NextDay()
    {
        yield return StartCoroutine(HandlePlayerTurn());
        yield return StartCoroutine(HandleDrones());
        yield return StartCoroutine(HandleZombies());
        yield return StartCoroutine(HandleEndOfDay());
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

        // 플레이어와 겹치지 않는 랜덤 타일 뽑기.
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

        // 구조물 렌더링 최적화
        OptimizeStructureRendering();
        
        // 타일 렌더링 최적화
        OptimizeTileRendering();
        
        // 캐시 정리
        CleanupVisibilityCache();
    }

    /// <summary>
    /// 구조물 렌더링을 최적화하는 함수
    /// SetActive 대신 Renderer 컴포넌트를 직접 제어하여 더 효율적으로 처리
    /// </summary>
    private void OptimizeStructureRendering()
    {
        List<StructureObject> structureObjects =
            objectsTransform.GetComponentsInChildren<StructureObject>(true).ToList();

        for (int i = 0; i < structureObjects.Count; i++)
        {
            StructureObject item = structureObjects[i];
            GameObject obj = item.gameObject;
            
            bool shouldBeVisible = sightTiles.Contains(item.CurTile);
            
            // 캐시된 상태와 다를 때만 렌더링 상태 변경
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
                // 첫 번째 실행 시 캐시 초기화
                SetObjectVisibility(obj, shouldBeVisible);
                _lastVisibilityState[obj] = shouldBeVisible;
            }
        }
    }

    /// <summary>
    /// 타일 렌더링을 최적화하는 함수
    /// 배치 처리를 통해 성능 향상
    /// </summary>
    private void OptimizeTileRendering()
    {
        var allTiles = GetAllTiles();
        var visibleTiles = new HashSet<Tile>(sightTiles);
        
        // 배치 처리를 위한 리스트
        var toShow = new List<GameObject>();
        var toHide = new List<GameObject>();
        
        for (int i = 0; i < allTiles.Count; i++)
        {
            Tile item = allTiles[i];
            GameObject tileObj = (GameObject)item.GameEntity;
            
            bool shouldBeVisible = visibleTiles.Contains(item);
            
            if (_lastVisibilityState.TryGetValue(tileObj, out bool lastState))
            {
                if (lastState != shouldBeVisible)
                {
                    if (shouldBeVisible)
                        toShow.Add(tileObj);
                    else
                        toHide.Add(tileObj);
                    
                    _lastVisibilityState[tileObj] = shouldBeVisible;
                }
            }
            else
            {
                if (shouldBeVisible)
                    toShow.Add(tileObj);
                else
                    toHide.Add(tileObj);
                
                _lastVisibilityState[tileObj] = shouldBeVisible;
            }
        }
        
        // 배치 처리로 성능 향상
        BatchSetVisibility(toShow, true);
        BatchSetVisibility(toHide, false);
    }

    /// <summary>
    /// 오브젝트의 가시성을 설정하는 함수
    /// SetActive 대신 Renderer 컴포넌트를 직접 제어
    /// </summary>
    private void SetObjectVisibility(GameObject obj, bool visible)
    {
        if (obj == null) return;
        
        // Renderer 컴포넌트 캐시
        if (!_rendererCache.TryGetValue(obj, out Renderer renderer))
        {
            renderer = obj.GetComponent<Renderer>();
            if (renderer != null)
                _rendererCache[obj] = renderer;
        }
        
        if (renderer != null)
        {
            // Renderer 컴포넌트를 직접 제어
            renderer.enabled = visible;
            
            // 자식 오브젝트들의 Renderer도 함께 제어
            Renderer[] childRenderers = obj.GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer != renderer)
                    childRenderer.enabled = visible;
            }
        }
        else
        {
            // Renderer가 없는 경우에만 SetActive 사용
            obj.SetActive(visible);
        }
        
        // 가시성 상태 추적
        if (visible)
            _visibleObjects.Add(obj);
        else
            _invisibleObjects.Add(obj);
    }

    /// <summary>
    /// 배치 처리를 통해 여러 오브젝트의 가시성을 한 번에 설정
    /// </summary>
    private void BatchSetVisibility(List<GameObject> objects, bool visible)
    {
        if (objects.Count == 0) return;
        
        // 렌더러별로 그룹화하여 배치 처리
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
        
        // 배치 처리
        foreach (var group in rendererGroups)
        {
            Renderer renderer = group.Key;
            renderer.enabled = visible;
            
            // 자식 렌더러들도 함께 처리
            Renderer[] childRenderers = renderer.GetComponentsInChildren<Renderer>();
            foreach (var childRenderer in childRenderers)
            {
                if (childRenderer != renderer)
                    childRenderer.enabled = visible;
            }
        }
        
        // SetActive가 필요한 오브젝트들 처리
        foreach (var obj in objects)
        {
            if (!_rendererCache.ContainsKey(obj))
            {
                obj.SetActive(visible);
            }
        }
    }

    /// <summary>
    /// 가시성 캐시를 정리하는 함수
    /// 메모리 누수 방지
    /// </summary>
    private void CleanupVisibilityCache()
    {
        // 파괴된 오브젝트들 제거
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
        
        // 캐시 크기 제한 (메모리 사용량 제어)
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

    /// <summary>
    /// 렌더링 최적화 캐시를 완전히 초기화하는 함수
    /// </summary>
    public void ClearRenderingCache()
    {
        _rendererCache.Clear();
        _lastVisibilityState.Clear();
        _visibleObjects.Clear();
        _invisibleObjects.Clear();
    }

    public void SightCheckInit()
    {
        OcclusionCheck(GetTileFromCoords(new Coords(0, 0)));
    }

    public List<Tile> GetPlayerSightTiles()
    {
        var list = GetTilesInRange(Player.TileController.Model, mapSettings.playerSightRange);
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
        
        _pos.y += mapSettings.playerSpawnHeight; // 임시로 playerSpawnHeight 사용
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

    private IEnumerator GenerateMapObjects()
    {
        LoadGameData();
        InitializeManagers();
        SpawnPlayer();
        GenerateStructures();
        if (zombieManager != null)
            zombieManager.SpawnZombies(mapData.zombieCount, mapData);
        InitializeFogOfWar();
        StartCoroutine(RandomTileResource(mapData.resourcePercent));
        yield return null;
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
        }
    }

    private void InitializeFogOfWar()
    {
        csFogWar.instance.InitializeMapControllerObjects(Player.gameObject, mapData.fogSightRange);
        DeselectAllBorderTiles();
    }

    private void InitializeManagers()
    {
        // 자동으로 매니저들을 찾아서 바인딩
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

        // 매니저가 없으면 자동 생성
        if (zombieManager == null)
        {
            zombieManager = gameObject.AddComponent<ZombieManager>();
            Debug.Log("ZombieManager 자동 생성됨");
        }
        
        if (droneManager == null)
        {
            droneManager = gameObject.AddComponent<DroneManager>();
            Debug.Log("DroneManager 자동 생성됨");
        }
        
        if (structureManager == null)
        {
            structureManager = gameObject.AddComponent<StructureManager>();
            Debug.Log("StructureManager 자동 생성됨");
        }

        // 매니저 초기화
        if (zombieManager != null)
            zombieManager.Initialize(this, zombiesTransform, mapPrefab);
        
        if (droneManager != null)
            droneManager.Initialize(this, Player, mapTransform, mapPrefab);
        
        if (structureManager != null)
            structureManager.Initialize(this, objectsTransform, mapPrefab);
    }

    private IEnumerator RandomTileResource(float _percent)
    {
        bool complete = false;

        List<TileBase> tileBaseList = GetAllTiles()
            .Select(x => ((GameObject)x.GameEntity).GetComponent<TileBase>())
            .ToList();

        float randomTileCount = tileBaseList.Count - (tileBaseList.Count * (_percent * 0.01f));

        for (int i = 0; i < randomTileCount; ++i)
        {
            int randNum = Random.Range(0, tileBaseList.Count);
            tileBaseList.RemoveAt(randNum);
        }

        for (int i = 0; i < tileBaseList.Count; i++)
        {
            TileBase tile = tileBaseList[i];
            tile.SpawnRandomResource();

            if (i == tileBaseList.Count - 1)
                complete = true;
        }

        yield return new WaitUntil(() => complete);

        OcclusionCheck(Player.TileController.Model);
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
        StartCoroutine(FloatingAnimation());

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

    private IEnumerator FloatingAnimation()
    {
        yield return new WaitUntil(() => Player != null);
        Player.StartFloatingAnimation();
    }

    private IEnumerator HandlePlayerTurn()
    {
        Player.ChangeClockBuffDuration();
        
        if (Player.MovePath != null)
        {
            yield return StartCoroutine(Player.ActionDecision(targetTileController));
        }
        else
        {
            DeselectAllBorderTiles();
        }
    }

    private IEnumerator HandleDrones()
    {
        yield return StartCoroutine(droneManager.HandleDrones());
    }

    private IEnumerator HandleZombies()
    {
        yield return StartCoroutine(zombieManager.HandleZombies());
    }

    private IEnumerator HandleEndOfDay()
    {
        // 이동 거리 충전
        Player.SetHealth(true);
        Player.TileEffectCheck();
        OcclusionCheck(Player.TileController.Model);
        
        yield return null;
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

    /// <summary>
    /// 같은 그룹 타일이 모여있는 곳에 마우스를 올리면 그룹 전체를 선택을 하게 해주는 함수. 현재 사용 중이지 않다.
    /// </summary>
    /// <param name="tile"></param>
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
        // landform rocks도 거르면 건물 잔해도 거를 수 있음
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