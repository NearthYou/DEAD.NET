using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexamap;
using DG.Tweening;
using TMPro;

public class ZombieSwarm : MonoBehaviour
{
    public int zombieCount;
    public int foodCount;
    public int drinkCount;
    public GameObject equipment;
    public bool isChasingPlayer;
    public DisturbanceMachine nearthDistrubtor;
    public int remainStunTime;

    public float zombieMovePossibility;
    public float zombieStayPossibility;
    public float specailZombiePossibility;
    public float zombieMinCount;
    public float zombieMaxCount;

    public int moveCost = 1;
    public Tile curTile;
    public Tile lastTile;
    public Tile targetTile;
    List<Coords> movePath;
    TMP_Text zombieCountTMP;

    //public SpecialZombie[] specialZombies;

    public void Init(Tile tile)
    {
        App.instance.GetDataManager().gameData.TryGetValue("Data_Zombie_Move_Possibility", out GameData move);
        App.instance.GetDataManager().gameData.TryGetValue("Data_Zombie_Stay_Possibility", out GameData stay);
        App.instance.GetDataManager().gameData.TryGetValue("Data_SpecialZombie_Possibility", out GameData special);
        App.instance.GetDataManager().gameData.TryGetValue("Data_MinCount_ZombieSwarm", out GameData min);
        App.instance.GetDataManager().gameData.TryGetValue("Data_MaxCount_ZombieSwarm", out GameData max);

        zombieMovePossibility = move.value;
        zombieStayPossibility = stay.value;
        specailZombiePossibility = special.value;
        zombieMinCount = min.value;
        zombieMaxCount = max.value;

        zombieCount = (int)Random.Range(zombieMinCount, zombieMaxCount);
        curTile = tile;
        lastTile = curTile;
        zombieCountTMP = ((GameObject)curTile.GameEntity).GetComponent<TileInfo>().GetZombieText();
        CurrentTileInfoUpdate(curTile);
    }

    public void DetectionAndAct()
    {
        // ���� ��Ʈ�ѷ����� ���� ������.
        isChasingPlayer = App.instance.GetMapManager().CalculateDistanceToPlayer(curTile, 2);
        nearthDistrubtor = App.instance.GetMapManager().CalculateDistanceToDistrubtor(curTile, 2);
        ActionDecision();
    }

    public void ActionDecision()
    {
        if (remainStunTime > 0)
        {
            //Debug.Log(gameObject.name + "�� ������ ������ ���ϰ� �ִ�.");
            remainStunTime--;
            return;
        }

        if (nearthDistrubtor != null)
        {
            Debug.Log(gameObject.name + "�� �����⸦ �߰�!");
            StartCoroutine(MoveToTarget(nearthDistrubtor.currentTile));

            return;
        }

        if (isChasingPlayer)
        {
            Debug.Log(gameObject.name + "�� �÷��̾ �߰�!");
            StartCoroutine(MoveToTarget(App.instance.GetMapManager().GetPlayerLocationTile()));
        }
        else
        {
            var randomInt = GetRandom();
            if (randomInt == 0)
            {
                //Debug.Log(gameObject.name + "�� ��ó���� ������ �ٴϰ� �ִ�...");
                StartCoroutine(MoveToRandom());
            }
            else
            {
                //Debug.Log(gameObject.name + "�� �������� ������ �ʴ´�...");
            }
        }
    }

    public IEnumerator MoveToTarget(Tile target, int walkCount = 1, float time = 0.5f)
    {
        movePath = AStar.FindPath(curTile.Coords, target.Coords);

        Tile targetTile;
        Vector3 targetPos;

        for (int i = 0; i < walkCount; i++)
        {
            if (movePath.Count <= 0)
                break;

            targetTile = App.instance.GetMapManager().GetTileFromCoords(movePath[i]);
            targetPos = ((GameObject)targetTile.GameEntity).transform.position;
            targetPos.y += 1;
            gameObject.transform.DOMove(targetPos, time);
            yield return new WaitForSeconds(time);
            curTile = targetTile;
        }
        App.instance.GetMapManager().CheckSumZombies();
        CurrentTileInfoUpdate(curTile);
        CurrentTileInfoUpdate(lastTile);
        lastTile = curTile;
    }

    public IEnumerator MoveToRandom(int num = 1, float time = 0.5f)
    {
        var candidate = App.instance.GetMapManager().GetTilesInRange(curTile, num);
        int rand = UnityEngine.Random.Range(0, candidate.Count);

        while (((GameObject)candidate[rand].GameEntity).gameObject.layer == 8)
        {
            candidate.RemoveAt(rand);
            rand = UnityEngine.Random.Range(0, candidate.Count);
        }

        var targetPos = ((GameObject)candidate[rand].GameEntity).transform.position;
        targetPos.y += 1;

        yield return gameObject.transform.DOMove(targetPos, time);

        curTile = candidate[rand];
        App.instance.GetMapManager().CheckSumZombies();
        CurrentTileInfoUpdate(curTile);
        CurrentTileInfoUpdate(lastTile);
        lastTile = curTile;
    }

    public void CurrentTileInfoUpdate(Tile tile)
    {
        if (tile == curTile)
            zombieCountTMP.text = "���� �� " + zombieCount + "ü";
        else
            zombieCountTMP.text = "�� �� ����";
    }

    public void SumZombies(ZombieSwarm zombie)
    {
        zombieCount += zombie.zombieCount;
        foodCount += zombie.foodCount;
        drinkCount += zombie.drinkCount;
    }

    public int GetRandom()
    {
        float percentage = zombieMovePossibility + zombieStayPossibility;
        float probability = zombieMovePossibility / percentage;
        float rate = percentage - (percentage * probability);
        int tmp = (int)Random.Range(0, percentage);

        if (tmp <= rate - 1)
        {
            return 0;
        }
        return 1;
    }

    public void MoveTargetCoroutine(Tile tile)
    {
        StartCoroutine(MoveToTarget(tile));
    }
}
