using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Hexamap;
using UnityEngine.Rendering.UI;
using Yarn.Compiler;
using Cysharp.Threading.Tasks;
using Random = System.Random;

public class MapManager : ManagementBase
{
    [Header("UI Components")]
    public MapUiController mapUIController;
    public ResourceManager resourceManager;
    public ParticleLODManager particleLODManager;
    
    [Header("Input Settings")]
    [SerializeField] private ETileMouseState mouseState;
    [SerializeField] private MapData mapData;
    
    [Header("Input State")]
    public bool mouseIntreractable;
    
    // UI 및 입력 관련 컴포넌트
    private Camera mainCamera;
    private MapCamera mapCineCamera;
    private TileController curTileController;
    private TileController cameraTarget;
    
    // 입력 상태 관리
    private bool canPlayerMove;
    private bool isDronePrepared;
    private bool isDisturbtorPrepared;
    private bool isCameraMove;
    private bool isTundraTile;
    
    // 게임 상태 (UI 표시용)
    private StructureBase curStructure;

    private void Update()
    {
        SetETileMoveState();

        if (mouseState != ETileMouseState.Nothing)
        {
            if (isCameraMove)
                GetCameraCenterTile();

            MouseOverEvents();
        }
    }

    private async UniTask GetAdditiveSceneObjectsAsync()
    {
        await UniTask.WaitForEndOfFrame(this);
        await UniTask.Delay(100);
        
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        await UniTask.Yield();
        
        mapUIController = GameObject.FindGameObjectWithTag("MapUi").GetComponent<MapUiController>();
        await UniTask.Yield();
        
        mapCineCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<MapCamera>();
        await UniTask.Delay(100);
        
        resourceManager = GameObject.FindGameObjectWithTag("Resource").GetComponent<ResourceManager>();
        await UniTask.Delay(100);
        
        if (MapController.Instance != null)
        {
            MapController.Instance.InputMapData(mapData);
            await UniTask.WaitUntil(() => MapController.Instance != null);
            await UniTask.Delay(50);
            
            await MapController.Instance.GenerateMapAsync();
            await UniTask.Delay(200);
            
            await mapCineCamera.GetMapInfoAsync();
            await UniTask.Delay(100);

            MapController.Instance.SightCheckInit();
            await UniTask.Delay(200);
        }
        
        AllowMouseEvent(true);
        
        InitializeParticleLODManager();
    }

    public void GetAdditiveSceneObjectsCoroutine()
    {
        GetAdditiveSceneObjectsAsync().Forget();
    }

    private void MouseOverEvents()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            MapController.Instance.DeselectAllBorderTiles();
            return;
        }

        RaycastHit hit;
        TileController tileController;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskTile))
        {
            tileController = hit.transform.parent.GetComponent<TileController>();

            MapController.Instance.DeselectAllBorderTiles();

            if (!MapController.Instance.CheckPlayersView(tileController))
            {
                mapUIController.FalseTileInfo();
                return;
            }
            
            switch (mouseState)
            {
                case ETileMouseState.CanClick:
                    MapController.Instance.DefaultMouseOverState(tileController);

                    if (tileController != curTileController)
                        mapUIController.FalseTileInfo();
                    break;

                case ETileMouseState.CanPlayerMove:
                    MapController.Instance.TilePathFinderSurroundings(tileController);
                    MapController.Instance.AddSelectedTilesList(tileController);
                    break;

                case ETileMouseState.DronePrepared:
                    if (isDisturbtorPrepared)
                    {
                        MapController.Instance.DisturbtorPathFinder(tileController);
                    }
                    else
                    {
                        MapController.Instance.ExplorerPathFinder(tileController, 5);
                    }
                    break;
            }

            curTileController = tileController;
        }
        else
        {
            MapController.Instance.DeselectAllBorderTiles();
            mapUIController.FalseTileInfo();
        }

        MouseClickEvents();
    }

    private void MouseClickEvents()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        int onlyLayerMaskPlayer = 1 << LayerMask.NameToLayer("Player");
        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskPlayer))
            {
                if (!isDronePrepared)
                    canPlayerMove = MapController.Instance.PlayerCanMoveCheck();
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskTile))
            {
                TileController tileController = hit.transform.parent.GetComponent<TileController>();

                if (!canPlayerMove && !isDronePrepared)
                {
                    tileController.GetComponent<TileBase>().TileInfoUpdate();
                    mapUIController.TrueTileInfo();
                }
                else if (canPlayerMove)
                {
                    if (MapController.Instance.SelectPlayerMovePoint(tileController))
                    {
                        mapUIController.OnPlayerMovePoint(tileController.transform);
                        MapController.Instance.MovePointerOn(tileController.transform.position);
                        canPlayerMove = false;
                    }
                    else
                        return;
                }
                else if (isDronePrepared)
                {
                    if (isDisturbtorPrepared)
                    {
                        MapController.Instance.SelectTileForDisturbtor(tileController);
                    }
                    else
                    {
                        MapController.Instance.SelectTileForExplorer(tileController);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            MapController.Instance.DeselectAllBorderTiles();

            if (canPlayerMove)
            {
                canPlayerMove = false;
            }

            if (isDronePrepared)
            {
                if (isDisturbtorPrepared)
                {
                    MapController.Instance.PreparingDistrubtor(false);
                }
                else
                {
                    MapController.Instance.PreparingExplorer(false);
                }
            }

            MovePathDelete();
        }

        if (Input.GetMouseButton(2))
        {
            isCameraMove = true;
        }
        else if (Input.GetMouseButtonUp(2))
        {
            isCameraMove = false;
        }
    }

    private void SetETileMoveState()
    {
        if (!mouseIntreractable)
            mouseState = ETileMouseState.Nothing;
        else if (!canPlayerMove && !isDronePrepared)
            mouseState = ETileMouseState.CanClick;
        else if (canPlayerMove)
            mouseState = ETileMouseState.CanPlayerMove;
        else if (isDronePrepared)
            mouseState = ETileMouseState.DronePrepared;
    }

    public async UniTask NextDayCoroutineAsync()
    {
        await MapController.Instance.NextDayAsync();
        resourceManager.GetResource(MapController.Instance.Player.TileController);
        mapUIController.OffPlayerMovePoint();
        MapController.Instance.OnlyMovePointerOff();
        
        CheckRoutine();
    }

    public bool CheckCanInstallDrone()
    {
        if (mouseState == ETileMouseState.CanClick)
        {
            return true;
        }

        return false;
    }

    public void AllowMouseEvent(bool isAllow)
    {
        mouseIntreractable = isAllow;
        canPlayerMove = false;
        isDronePrepared = false;
        isDisturbtorPrepared = false;
    }

    public override EManagerType GetManagemetType()
    {
        return EManagerType.MAP;
    }

    public void SetMapCameraPriority(bool _set)
    {
        mapCineCamera.SetPrioryty(_set);
    }

    public void CheckRoutine()
    {
        CheckZombies();
        CheckStructureNeighbor();
        AllowMouseEvent(true);
    }

    public void CheckZombies()
    {
        if (MapController.Instance.CheckZombies())
        {
            UIManager.instance.GetAlertController().SetAlert("caution", true);
        }
    }

    public void CheckStructureNeighbor()
    {
        var structure = MapController.Instance.SensingStructure();
        if (structure != null)
        {
            if (structure is Tower)
                if (UIManager.instance.GetInventoryController().CheckNetCardUsage() == false) return;

            if (structure.IsUse == false)
                UIManager.instance.GetPageController().SetSelectPage("structureSelect", structure);
        }
    }

    public void CheckLandformPlayMusic()
    {
        var curTile = MapController.Instance.Player.TileController.GetComponent<TileBase>();

        switch (curTile.TileData.English)
        {
            case "None":
                App.instance.GetSoundManager().PlayBGM("Ambience_City");
                break;
            case "Jungle":
                App.instance.GetSoundManager().PlayBGM("Ambience_Jungle");
                break;
            case "Desert":
                App.instance.GetSoundManager().PlayBGM("Ambience_Desert");
                break;
            case "Tundra":
                App.instance.GetSoundManager().PlayBGM("Ambience_Tundra");
                break;
        }
    }

    public void NormalStructureResearch(StructureBase structure)
    {
        int randomNumber = UnityEngine.Random.Range(1, 4);

        if (randomNumber == 3)
            MapController.Instance.SpawnStructureZombies(structure.Colleagues);

        if (isTundraTile)
        {
            UIManager.instance.GetPageController().SetResultPage("SEARCH_TUNDRA", false);
            MapController.Instance.Player.SetHealth(false);
        }

        MovePathDelete();

        structure.structureModel.GetComponent<StructureFade>().FadeIn();
        structure.Colleagues.ForEach(tile => tile.ResourceUpdate(true));

        MapController.Instance.SpawnSpecialItemRandomTile(structure.Colleagues);
        curStructure = structure;
    }

    public void ResearchCancel(StructureBase structure)
    {
    }

    public void MovePathDelete()
    {
        if (MapController.Instance.IsMovePathSaved() == false)
            return;

        mapUIController.OffPlayerMovePoint();
        MapController.Instance.MovePointerOff();
        MapController.Instance.DeletePlayerMovePath();
    }

    // public void TutorialTileCheck()
    // {
    //     if (mapController.Player.TileController.GetComponent<TileBase>().TileData.English == "None")
    //     {
    //         UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_NETWORKCHIP");
    //         isVisitNoneTile = true;
    //     }
    // }

    public bool SensingSignalTower()
    {
        return MapController.Instance.SensingSignalTower();
    }

    public bool SensingProductionStructure()
    {
        return MapController.Instance.SensingProductionStructure();
    }

    public bool SignalTowerQuestCheck()
    {
        if (curStructure == null)
            return false;

        if (curStructure.VisitDay != UIManager.instance.GetNoteController().dayCount)
        {
            return true;
        }
        else
        {
            return false;
        }
    }

    public void GetCameraCenterTile()
    {
        if (mainCamera == null)
            mainCamera = Camera.main;
            
        if (mainCamera == null) return;
        
        Vector3 centerPos = new Vector3(mainCamera.pixelWidth / 2, mainCamera.pixelHeight / 2);
        Ray ray = mainCamera.ScreenPointToRay(centerPos);

        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, onlyLayerMaskTile))
        {
            var target = hit.transform.parent.GetComponent<TileController>();

            if (target == null)
            {
                return;
            }

            if (cameraTarget != target)
            {
                cameraTarget = target;
                StartCoroutine(DelayedOcclusionCheck());
            }
        }
    }
    
    private IEnumerator DelayedOcclusionCheck()
    {
        yield return null;
        if (cameraTarget != null)
        {
            MapController.Instance.OcclusionCheck(cameraTarget.Model);
        }
    }
    
    public void TundraTileCheck()
    {
        isTundraTile = true;
    }

    public void EtherResourceCheck()
    {
        var resources = resourceManager.GetLastResources();

        if (resources.Count == 0 || resources == null)
            return;
        
        if(resources.Find(x=> x.ItemBase.data.Code == "ITEM_GAS") != null)
        {
            UIManager.instance.GetPageController().SetResultPage("ACIDENT_ETHER", false);
            MapController.Instance.Player.SetHealth(false);
        }
    }

    public bool IsJungleTile(TileController _tileController)
    {
        if (_tileController.GetComponent<TileBase>().TileType == ETileType.Jungle)
            return true;
        else
        {
            return false;
        }
    }

    public void SetIsDronePrepared(bool _isDronePrepared, string type)
    {
        isDronePrepared = _isDronePrepared;
        
        if(type == "Distrubtor")
            isDisturbtorPrepared = true;
        else
            isDisturbtorPrepared = false;
    }

    public void InvocationExplorers()
    {
        MapController.Instance.InvocationExplorers();
    }
    
    private void InitializeParticleLODManager()
    {
        if (particleLODManager == null)
        {
            GameObject lodManagerObject = new GameObject("ParticleLODManager");
            lodManagerObject.transform.SetParent(transform);
            particleLODManager = lodManagerObject.AddComponent<ParticleLODManager>();
        }
    }
    
    public void UpdateTileParticles(GameObject tileObject)
    {
        if (particleLODManager != null)
        {
            particleLODManager.ForceUpdateTileParticles(tileObject);
        }
    }
    
    public void UpdateAllParticleLOD()
    {
        if (particleLODManager != null)
        {
            particleLODManager.ForceUpdateAllParticles();
        }
    }
    
    public async UniTask UpdateAllParticleLODAsync()
    {
        if (particleLODManager != null)
        {
            await particleLODManager.ForceUpdateAllParticlesAsync();
        }
    }
    
    public void SetParticleLODSettings(float highDistance, float mediumDistance, float lowDistance)
    {
        if (particleLODManager != null)
        {
            particleLODManager.SetLODSettings(highDistance, mediumDistance, lowDistance);
        }
    }
    
    public void SetParticleLODEnabled(bool enabled)
    {
        if (particleLODManager != null)
        {
            particleLODManager.SetParticleLODEnabled(enabled);
        }
    }
}