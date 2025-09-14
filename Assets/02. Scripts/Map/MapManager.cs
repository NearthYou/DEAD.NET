using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Hexamap;
using UnityEngine.Rendering.UI;
using Yarn.Compiler;
using Random = System.Random;

public class MapManager : ManagementBase
{
    public MapUiController mapUIController;
    public MapController mapController;
    public ResourceManager resourceManager;
    public bool mouseIntreractable;

    [SerializeField] ETileMouseState mouseState;

    [Header("밸런스 테스트 용")] [Space(5f)] [SerializeField]
    MapData mapData;
    
    Camera mainCamera;
    MapCamera mapCineCamera;
    TileController curTileController;
    StructureBase curStructure;

    bool canPlayerMove;
    bool isDronePrepared;
    bool isDisturbtorPrepared;
    bool isCameraMove;
    bool isTundraTile;

    TileBase structureTileBase;

    private TileController cameraTarget;

    void Update()
    {
        SetETileMoveState();

        if (mouseState != ETileMouseState.Nothing)
        {
            if (isCameraMove)
                GetCameraCenterTile();

            MouseOverEvents();
        }
        
        //if(Input.GetKeyDown(KeyCode.Z))
            //UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_FINDOR");
        //if(Input.GetKeyDown(KeyCode.X))
            //UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_DISTURBE");
    }

    IEnumerator GetAdditiveSceneObjects()
    {
        yield return new WaitForEndOfFrame();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mapUIController = GameObject.FindGameObjectWithTag("MapUi").GetComponent<MapUiController>();
        mapController = GameObject.FindGameObjectWithTag("MapController").GetComponent<MapController>();

        mapController.InputMapData(mapData);

        yield return new WaitUntil(() => mapController != null);
        StartCoroutine(mapController.GenerateMap());
        mapCineCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<MapCamera>();

        AllowMouseEvent(true);
        resourceManager = GameObject.FindGameObjectWithTag("Resource").GetComponent<ResourceManager>();
        StartCoroutine(mapCineCamera.GetMapInfo());

        mapController.SightCheckInit();
    }

    public void GetAdditiveSceneObjectsCoroutine()
    {
        StartCoroutine(GetAdditiveSceneObjects());
    }

    void MouseOverEvents()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            mapController.DeselectAllBorderTiles();
            return;
        }

        RaycastHit hit;
        TileController tileController;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskTile))
        {
            tileController = hit.transform.parent.GetComponent<TileController>();

            mapController.DeselectAllBorderTiles();

            if (!mapController.CheckPlayersView(tileController))
            {
                mapUIController.FalseTileInfo();
                return;
            }
            
            switch (mouseState)
            {
                case ETileMouseState.CanClick:
                    mapController.DefaultMouseOverState(tileController);

                    if (tileController != curTileController)
                        mapUIController.FalseTileInfo();
                    break;

                case ETileMouseState.CanPlayerMove:
                    mapController.TilePathFinderSurroundings(tileController);
                    mapController.AddSelectedTilesList(tileController);
                    break;

                case ETileMouseState.DronePrepared:
                    if (isDisturbtorPrepared)
                    {
                        mapController.DisturbtorPathFinder(tileController);
                    }
                    else
                    {
                        mapController.ExplorerPathFinder(tileController, 5);
                    }

                    break;
            }

            curTileController = tileController;
        }
        else
        {
            mapController.DeselectAllBorderTiles();
            mapUIController.FalseTileInfo();
        }

        MouseClickEvents();
    }

    void MouseClickEvents()
    {
        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        int onlyLayerMaskPlayer = 1 << LayerMask.NameToLayer("Player");
        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Input.GetMouseButtonDown(0))
        {
            // 플레이어를 클릭한 경우
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskPlayer))
            {
                if (!isDronePrepared)
                    canPlayerMove = mapController.PlayerCanMoveCheck();
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
                    if (mapController.SelectPlayerMovePoint(tileController))
                    {
                        mapUIController.OnPlayerMovePoint(tileController.transform);
                        mapController.MovePointerOn(tileController.transform.position);
                        canPlayerMove = false;
                    }
                    else
                        return;
                }
                else if (isDronePrepared)
                {
                    if (isDisturbtorPrepared)
                    {
                        mapController.SelectTileForDisturbtor(tileController);
                    }
                    else
                    {
                        mapController.SelectTileForExplorer(tileController);
                    }
                }
            }
        }

        if (Input.GetMouseButtonDown(1))
        {
            mapController.DeselectAllBorderTiles();

            if (canPlayerMove)
            {
                canPlayerMove = false;
            }

            // 목적지 정한 이후 취소 가능

            if (isDronePrepared)
            {
                if (isDisturbtorPrepared)
                {
                    mapController.PreparingDistrubtor(false);
                }
                else
                {
                    mapController.PreparingExplorer(false);
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

    void SetETileMoveState()
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

    public IEnumerator NextDayCoroutine()
    {
        yield return StartCoroutine(mapController.NextDay());
        resourceManager.GetResource(mapController.Player.TileController);
        mapUIController.OffPlayerMovePoint();
        mapController.OnlyMovePointerOff();
        
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

        // 튜토리얼 네트워크 칩
        // if(isVisitNoneTile == false)
        // {
        //     TutorialTileCheck();
        // }
        
        AllowMouseEvent(true);
    }

    public void CheckZombies()
    {
        if (mapController.CheckZombies())
        {
            UIManager.instance.GetAlertController().SetAlert("caution", true);
        }
        else
            return;
    }

    /// <summary>
    /// 현재 타일이 구조물 인접타일인지 확인
    /// </summary>
    public void CheckStructureNeighbor()
    {
        var structure = mapController.SensingStructure();
        if (structure != null)
        {
            if (structure is Tower)
                if (UIManager.instance.GetInventoryController().CheckNetCardUsage() == false) return;

            if (structure.IsUse == false)
                UIManager.instance.GetPageController().SetSelectPage("structureSelect", structure);
        }
        else
        {
            return;
        }
    }

    public void CheckLandformPlayMusic()
    {
        var curTile = mapController.Player.TileController.GetComponent<TileBase>();

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
            mapController.SpawnStructureZombies(structure.Colleagues);

        // 플레이어 체력 0으로 만들어서 경로 선택 막기
        if (isTundraTile)
        {
            UIManager.instance.GetPageController().SetResultPage("SEARCH_TUNDRA", false);
            mapController.Player.SetHealth(false);
        }

        // 경로 삭제
        MovePathDelete();

        structure.structureModel.GetComponent<StructureFade>().FadeIn();
        structure.Colleagues.ForEach(tile => tile.ResourceUpdate(true));

        mapController.SpawnSpecialItemRandomTile(structure.Colleagues);
        curStructure = structure;
    }

    public void ResearchCancel(StructureBase structure)
    {
    }

    public void MovePathDelete()
    {
        if (mapController.IsMovePathSaved() == false)
            return;

        mapUIController.OffPlayerMovePoint();
        mapController.MovePointerOff();
        mapController.DeletePlayerMovePath();
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
        return mapController.SensingSignalTower();
    }

    public bool SensingProductionStructure()
    {
        return mapController.SensingProductionStructure();
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

    // 카메라 정중앙 좌표를 반환하는 함수
    public void GetCameraCenterTile()
    {
        Vector3 centerPos = new Vector3(Camera.main.pixelWidth / 2, Camera.main.pixelHeight / 2);
        Ray ray = Camera.main.ScreenPointToRay(centerPos);

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
                mapController.OcclusionCheck(cameraTarget.Model);
            }
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
            mapController.Player.SetHealth(false);
        }
        else
        {
            return;
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
        mapController.InvocationExplorers();
    }
}