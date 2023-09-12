using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using Hexamap;

public enum ETileMouseState
{
    Nothing,
    CanClick,
    CanPlayerMove,
    DronePrepared,
}

public class MapManager : ManagementBase
{
    public MapUiController mapUIController;
    public ETileMouseState mouseState;
    public bool mouseIntreractable;

    Camera mainCamera;
    MapCamera mapCineCamera;
    MapController mapController;
    ResourceManager resourceManager;
    TileController curTileController;

    bool isPlayerSelected;
    bool isDronePrepared;
    bool isDisturbtor;

    void Update()
    {
        SetETileMoveState();

        if (mouseState != ETileMouseState.Nothing)
        {
            MouseOverEvents();
        }
    }

    IEnumerator GetAdditiveSceneObjects()
    {
        yield return new WaitForEndOfFrame();
        mainCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        mapUIController = GameObject.FindGameObjectWithTag("MapUi").GetComponent<MapUiController>();
        mapController = GameObject.FindGameObjectWithTag("MapController").GetComponent<MapController>();

        yield return new WaitUntil(() => mapController != null);
        mapController.GenerateMap();
        mapCineCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<MapCamera>();
        resourceManager = GameObject.FindGameObjectWithTag("Resource").GetComponent<ResourceManager>();
        StartCoroutine(mapCineCamera.GetMapInfo());
    }

    public void GetAdditiveSceneObjectsCoroutine()
    {
        StartCoroutine(GetAdditiveSceneObjects());
    }

    /// <summary>
    /// Raytracing�� ���� ���콺 ���� ��ġ�� �´� Ÿ���� ������ �������ų�, Ÿ���� ���� ������Ʈ�� Ȱ��ȭ��Ű�� �Լ�.
    /// </summary>
    void MouseOverEvents()
    {
        RaycastHit hit;
        TileController tileController;

        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskTile))
        {
            tileController = hit.transform.parent.GetComponent<TileController>();

            mapController.DeselectAllBorderTiles();

            if (!mapController.CheckPlayersView(tileController))
                return;

            switch (mouseState)
            {
                case ETileMouseState.CanClick:
                    mapController.DefalutMouseOverState(tileController);

                    if (tileController != curTileController)
                        mapUIController.FalseTileInfo();

                    break;

                case ETileMouseState.CanPlayerMove:
                    mapController.TilePathFinder(tileController);
                    mapController.AddSelectedTilesList(tileController);
                    break;
                case ETileMouseState.DronePrepared:
                    if (isDisturbtor)
                    {
                        mapController.DisturbtorPathFinder(tileController);
                    }
                    else
                    {
                        mapController.TilePathFinder(tileController, 5);
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

    /// <summary>
    /// �÷��̾� �̵�, ������, Ž���� ��ġ �� ���콺 Ŭ���̺�Ʈ���� ���� �Լ�. Raycast ���
    /// </summary>
    void MouseClickEvents()
    {
        /*        if (EventSystem.current.IsPointerOverGameObject())
                {
                    return;
                }*/

        RaycastHit hit;
        Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

        int onlyLayerMaskPlayer = 1 << LayerMask.NameToLayer("Player");
        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Input.GetMouseButtonDown(0))
        {

            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskPlayer))
            {
                if (!isDronePrepared && !mapUIController.MovePointActivate())
                    isPlayerSelected = mapController.PlayerCanMoveCheck();
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskTile))
            {
                TileController tileController = hit.transform.parent.GetComponent<TileController>();

                if (!isPlayerSelected && !isDronePrepared)
                {
                    mapUIController.TrueTileInfo(tileController.transform.position);
                }
                else if (isPlayerSelected)
                {

                    if (mapController.SelectPlayerMovePoint(tileController))
                    {
                        mapUIController.OnPlayerMovePoint(tileController.transform);
                        isPlayerSelected = false;
                    }
                    else
                        return;

                }
                else if (isDronePrepared)
                {
                    if (isDisturbtor)
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

        // ��Ŭ�� �� ���� ���
        if (Input.GetMouseButtonDown(1))
        {
            mapController.DeselectAllBorderTiles();

            if (isPlayerSelected)
            {
                isPlayerSelected = false;
            }

            if (isDronePrepared)
            {
                if (isDisturbtor)
                {
                    mapController.PreparingDisturbtor(false);
                }
                else
                {
                    mapController.PreparingExplorer(false);
                }
            }
        }
    }

    /// <summary>
    /// ���콺 �̺�Ʈ ��Ʈ�� ���� ������ ������Ʈ �����ִ� �Լ�
    /// Update���� ȣ��
    /// </summary>
    void SetETileMoveState()
    {
        if (!mouseIntreractable)
            mouseState = ETileMouseState.Nothing;

        else if (!isPlayerSelected && !isDronePrepared)
            mouseState = ETileMouseState.CanClick;

        else if (isPlayerSelected)
            mouseState = ETileMouseState.CanPlayerMove;

        else if (isDronePrepared)
            mouseState = ETileMouseState.DronePrepared;
    }

    public IEnumerator NextDayCoroutine()
    {
        yield return StartCoroutine(mapController.NextDay());
        resourceManager.GetResource(mapController.Player.TileController);

        if (mapUIController.MovePointActivate())
            mapUIController.OffPlayerMovePoint();
    }

    /// <summary>
    /// �ൿ ���� ���� �� ��� ���� ��ư Ŭ�� ����
    /// </summary>
    /// <returns></returns>
    public bool CheckCanInstallDrone()
    {
        if (mouseState == ETileMouseState.CanClick)
        {
            return false;
        }
        return true;
    }

    public void AllowMouseEvent(bool isAllow)
    {
        isPlayerSelected = false;
        isDronePrepared = false;
        isDisturbtor = false;

        mouseIntreractable = isAllow;
    }

    public void OnTargetPointUI()
    {
        mapUIController.OnPlayerMovePoint(mapController.TargetPointTile.transform);
    }

    public override EManagerType GetManagemetType()
    {
        return EManagerType.MAP;
    }

    public void SetMapCameraPriority(bool _set)
    {
        mapCineCamera.SetPrioryty(_set);
    }

    // ó�� ���� �� �˻�, ���� NextDay �Լ� ������ �˻�.
    public void CheckResource()
    {
        if (resourceManager.CheckResource(mapController.Player.TileController))
        {
            // New �˸� ��
        }
        else
            return;
    }

    public void CheckZombies()
    {
        if (mapController.CheckZombies())
        {
            // ��� �˸� ��
        }
        else
            return;
    }
}
