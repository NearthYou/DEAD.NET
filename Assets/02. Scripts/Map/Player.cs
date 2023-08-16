using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexamap;
using DG.Tweening;

public class Player : MonoBehaviour
{
    public static Action<Tile> PlayerSightUpdate;

    int maxHealth = 1;
    int currentHealth;

    public int HealthPoint
    {
        get { return currentHealth; }
    }

    List<Coords> movePath;

    public List<Coords> MovePath
    {
        get { return movePath; }
    }


    TileController currentTileContorller;

    public TileController TileController
    {
        get { return currentTileContorller; }
    }

    void Start()
    {
        movePath = new List<Coords>();
        currentHealth = maxHealth;
        StartCoroutine(DelaySightGetInfo());
    }

    void SavePlayerMovePath(TileController tileController)
    {
        movePath = AStar.FindPath(currentTileContorller.Model.Coords, tileController.Model.Coords);

        //targetTileController = tileController;
        //arrow.OnEffect(tileController.transform);

        //isPlayerSelected = false;
        //isPlayerCanMove = false;

        //DeselectAllBorderTiles();
    }


    public IEnumerator MoveToTarget(TileController targetTileController, float time = 0.4f)
    {
        //isPlayerMoving = true;

        //DeselectAllBorderTiles();

        Tile targetTile;
        Vector3 targetPos;
        Vector3 lastTargetPos = targetTileController.transform.position;

        foreach (var item in movePath)
        {
            targetTile = MapController.instance.GetTileFromCoords(item);
            if (targetTile == null)
                break;

            targetPos = ((GameObject)targetTile.GameEntity).transform.position;
            targetPos.y += 0.5f;

            transform.DOMove(targetPos, time);
            currentHealth--;
            yield return new WaitForSeconds(time);
        }

        lastTargetPos.y += 0.5f;
        yield return transform.DOMove(lastTargetPos, time);
        yield return new WaitForSeconds(time);

        movePath.Clear();
        currentHealth = 0;

        UpdateCurrentTile(targetTileController);

        // MapManager�� �̵�
        //resourceManager.GetResource(playerLocationTileController);
        //arrow.OffEffect();
    }

    /// <summary>
    /// �÷��̾ �� �ִ� Ÿ���� ��ġ�� ������ ������ �� Ÿ���� ������ �Ѱ��ִ� �̺�Ʈ �Լ�
    /// </summary>
    /// <returns></returns>
    IEnumerator DelaySightGetInfo()
    {
        // AdditiveScene ������ 
        yield return new WaitForEndOfFrame();
        PlayerSightUpdate?.Invoke(currentTileContorller.Model);
    }

    public void UpdateCurrentTile(TileController tileController)
    {
        currentTileContorller = tileController;
        PlayerSightUpdate?.Invoke(currentTileContorller.Model);
    }

    public void UpdateMovePath(List<Coords> path)
    {
        movePath = path;
    }

    public void HealthCharging()
    {
        currentHealth = maxHealth;
    }
}
