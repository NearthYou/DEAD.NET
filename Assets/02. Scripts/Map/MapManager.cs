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
    Camera mapCamera;
    MapController mapController;
    MapUiController mapUIController;
    ResourceManager resourceManager;

    bool interactable;
    bool isPlayerSelected;
    bool isDronePrepared;
    bool isDisturbtor;

    ETileMouseState mouseState;

    public override EManagerType GetManagemetType()
    {
        return EManagerType.MAP;
    }

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
        mapCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<Camera>();
        //mapUIController ȣ��
        //noteAnim = GameObject.FindGameObjectWithTag("NoteAnim").GetComponent<NoteAnim>();
        //arrow = GameObject.FindGameObjectWithTag("MapUi").transform.GetChild(0).transform.Find("Map_Arrow").GetComponent<ArrowPointUI>();
    }

    public void CreateMap()
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

        Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);
        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskTile))
        {
            tileController = hit.transform.parent.GetComponent<TileController>();

            mapController.DeselectAllBorderTiles();

            switch (mouseState) 
            {
                case ETileMouseState.CanClick:
                    mapController.DefalutMouseOverState(tileController);
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
        }
        else
        {
            mapController.DeselectAllBorderTiles();

            //ui ��Ȱ��ȭ.
            //ui ��Ʈ�ѷ��� ���� �ű�

            /*            if (isUIOn)
                        {
                            currentUI.SetActive(false);
                            isUIOn = false;
                        }*/
        }

        //MouseClickEvents();
    }

    /// <summary>
    /// �÷��̾� �̵�, ������, Ž���� ��ġ �� ���콺 Ŭ���̺�Ʈ���� ���� �Լ�. Raycast ���
    /// </summary>
    void MouseClickEvents()
    {
        if (EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        RaycastHit hit;
        Ray ray = mapCamera.ScreenPointToRay(Input.mousePosition);

        int onlyLayerMaskPlayer = 1 << LayerMask.NameToLayer("Player");
        int onlyLayerMaskTile = 1 << LayerMask.NameToLayer("Tile");

        if (Input.GetMouseButtonDown(0))
        {
            if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskPlayer))
            {
                if(!isDronePrepared)
                    isPlayerSelected = mapController.PlayerCanMoveCheck();
            }
            else if (Physics.Raycast(ray, out hit, Mathf.Infinity, onlyLayerMaskTile))
            {
                TileController tileController = hit.transform.parent.GetComponent<TileController>();

                if (!isPlayerSelected && !isDronePrepared)
                {
                    // ui Ȱ��ȭ, UI ��Ʈ�ѷ��� �̻�
                    /*
                                        currentUI = GetUi(tileController);
                                        currentUI.SetActive(true);
                                        isUIOn = true;*/
                }
                else if (isPlayerSelected)
                {
                    mapController.SelectPlayerMovePoint(tileController);
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

    void SetETileMoveState()
    {
        if (!interactable)
            mouseState = ETileMouseState.Nothing;

        else if (!isPlayerSelected && !isDronePrepared)
            mouseState = ETileMouseState.CanClick;

        else if (isPlayerSelected)
            mouseState = ETileMouseState.CanPlayerMove;

        else if (isDronePrepared)
            mouseState = ETileMouseState.DronePrepared;
    }

    IEnumerator NextDayCoroutine()
    {
        yield return StartCoroutine(mapController.NextDay());
        resourceManager.GetResource(mapController.Player.TileController);
        ArrowUIOnOff(false);
    }

    public void ArrowUIOnOff(bool active)
    {
        //mapUIController~~~
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
}
