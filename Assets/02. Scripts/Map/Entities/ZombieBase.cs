using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using Hexamap;
using DG.Tweening;
using TMPro;
using Unity.VisualScripting;

public class ZombieData
{
    public int count;
    public float movePossibility;
    public float stayPossibility;
    public float specailZombiePossibility;
    public float minCount;
    public float maxCount;
}

[SelectionBase]
public class ZombieBase : MonoBehaviour
{
    [Header("Models")]
    [SerializeField] private GameObject[] zombieModels;
    
    [Header("Data")]
    public ZombieData zombieData = new ZombieData();
    
    [Header("State")]
    public Distrubtor nearthDistrubtor;
    public Tile curTile;
    public Tile lastTile;
    public Tile targetTile;
    public bool isChasingPlayer;
    
    private bool noneTileBuff;
    private int lastZombieCount;
    private List<Coords> movePath;
    private int remainStunTime;
    private int moveCost = 1;
    private int dectectionRange = 2;
    private int debuffCoolTime = 0;
    private Vector3 initScale = new Vector3(0, 0, 0);

    public void Init(Tile tile)
    {
        App.instance.GetDataManager().gameData.TryGetValue("Data_Zombie_Move_Possibility", out GameData move);
        App.instance.GetDataManager().gameData.TryGetValue("Data_Zombie_Stay_Possibility", out GameData stay);
        App.instance.GetDataManager().gameData.TryGetValue("Data_SpecialZombie_Possibility", out GameData special);
        App.instance.GetDataManager().gameData.TryGetValue("Data_MinCount_ZombieSwarm", out GameData min);
        App.instance.GetDataManager().gameData.TryGetValue("Data_MaxCount_ZombieSwarm", out GameData max);

        zombieData.movePossibility = move.value;
        zombieData.stayPossibility = stay.value;
        zombieData.specailZombiePossibility = special.value;
        zombieData.minCount = min.value;
        zombieData.maxCount = max.value;

        ZombieCountChoice();
        curTile = tile;
        lastTile = curTile;

        CheckTileEffect(curTile);
        CurrentTileUpdate(curTile);

        initScale = transform.localScale;
        lastZombieCount = zombieData.count;
    }

    ETileType CheckTileType(Tile _tile)
    {
        return ((GameObject)_tile.GameEntity).GetComponent<TileBase>().TileType;
    }

    void CheckTileEffect(Tile _tile)
    {
        switch (CheckTileType(_tile))
        {
            case ETileType.None:
                if (noneTileBuff == false)
                {
                    zombieData.count += 5;
                    ZombieModelChoice(zombieData.count);
                    noneTileBuff = true;
                }

                break;
            case ETileType.Desert:
                if (debuffCoolTime <= 0)
                {
                    debuffCoolTime = 1;
                }
                else if (debuffCoolTime > 0)
                {
                    debuffCoolTime -= 1;
                }

                break;
            case ETileType.Tundra:
                if (debuffCoolTime <= 0)
                {
                    debuffCoolTime = 1;
                }
                else if (debuffCoolTime > 0)
                {
                    debuffCoolTime -= 1;
                }

                break;
            case ETileType.Jungle:
                break;
            default:
                break;
        }
    }

    void ZombieCountChoice()
    {
        var randomInt = (int)Random.Range(zombieData.minCount, zombieData.maxCount);
        zombieData.count = randomInt;
        ZombieModelChoice(randomInt);
    }

    public void ZombieModelChoice(int count)
    {
        var num = (int)Mathf.Lerp(zombieData.minCount, zombieData.maxCount, 0.3f);
        num -= (int)zombieData.minCount;

        if (zombieData.minCount <= count && count <= zombieData.minCount + num)
        {
            for (int i = 0; i < zombieModels.Length; i++)
            {
                if (i == 0)
                    zombieModels[i].SetActive(true);
                else
                    zombieModels[i].SetActive(false);
            }
        }
        else if (zombieData.minCount + num <= count && count <= zombieData.minCount + num * 2)
        {
            for (int i = 0; i < zombieModels.Length; i++)
            {
                if (i == 1)
                    zombieModels[i].SetActive(true);
                else
                    zombieModels[i].SetActive(false);
            }
        }
        else
        {
            for (int i = 0; i < zombieModels.Length; i++)
            {
                if (i == 2)
                    zombieModels[i].SetActive(true);
                else
                    zombieModels[i].SetActive(false);
            }
        }
    }

    public void SizeUpCheck()
    {
        if (lastZombieCount != zombieData.count)
        {
            var scale = (zombieData.count / 10) * 0.1f;

            if (scale > 0.7f)
            {
                scale = 0.7f;
            }

            transform.localScale = initScale + new Vector3(scale, scale, scale);
            lastZombieCount = zombieData.count;
        }
    }

    public void SetValue(int cost, int _detectionRange)
    {
        moveCost = cost;
        dectectionRange = _detectionRange;
    }

    public void DetectionAndAct()
    {
        CheckTileEffect(curTile);

        isChasingPlayer = MapController.instance.CalculateDistanceToPlayer(curTile, dectectionRange);

        nearthDistrubtor = MapController.instance.CalculateDistanceToDistrubtor(curTile, dectectionRange);

        ActionDecision();
    }

    public void ActionDecision()
    {
        if (debuffCoolTime > 0)
        {
            return;
        }

        if (remainStunTime > 0)
        {
            remainStunTime--;
            return;
        }

        if (nearthDistrubtor != null)
        {
            //Debug.Log(gameObject.name + "가 교란기를 쫓아갑니다!");
            StartCoroutine(MoveToAttack(nearthDistrubtor.currentTile));
            return;
        }

        if (isChasingPlayer && !App.instance.GetMapManager().mapController.Player.GetIsClocking())
        {
            //Debug.Log(gameObject.name + "가 플레이어를 발견했습니다!");
            StartCoroutine(MoveToAttack(App.instance.GetMapManager().mapController.Player.TileController.Model));

            // 플레이어 바라보기
            var updatePos = App.instance.GetMapManager().mapController.Player.transform.position;
            updatePos.y += 0.6f;
            transform.LookAt(updatePos);
        }
        else
        {
            var randomInt = GetRandom();
            if (randomInt == 0)
            {
                StartCoroutine(MoveToRandom());
            }
        }
    }

    public IEnumerator MoveToAttack(Tile target, float time = 0.25f)
    {
        movePath = AStar.FindPath(curTile.Coords, target.Coords);

        Tile pointTile;
        Vector3 pointPos;

        if (movePath.Count == 0 && target == App.instance.GetMapManager().mapController.Player.TileController.Model)
        {
            // 플레이어가 1칸 내에 있는 경우
            AttackPlayer(App.instance.GetMapManager().mapController.Player);
        }
        else
        {
            if (movePath.Count == 0)
            {
                yield break;
            }

            for (int i = 0; i < moveCost; i++)
            {
                pointTile = App.instance.GetMapManager().mapController.GetTileFromCoords(movePath[i]);
                pointPos = ((GameObject)pointTile.GameEntity).transform.position;
                pointPos.y += 0.6f;

                gameObject.transform.DOMove(pointPos, time);

                yield return new WaitForSeconds(time);
                curTile = pointTile;

                if (movePath.Count == 1)
                    break;
            }
        }

        CurrentTileUpdate(lastTile);
        CurrentTileUpdate(curTile);

        lastTile = curTile;
    }

    public IEnumerator MoveToRandom(int num = 1, float time = 0.25f)
    {
        var candidate = MapController.instance.GetTilesInRange(curTile, num);
        int rand = Random.Range(0, candidate.Count);

        while (((GameObject)candidate[rand].GameEntity).gameObject.layer == 8)
        {
            candidate.RemoveAt(rand);
            rand = Random.Range(0, candidate.Count);
        }

        if (candidate[rand] == App.instance.GetMapManager().mapController.Player.TileController.Model)
            rand--;
        var targetPos = ((GameObject)candidate[rand].GameEntity).transform.position;
        targetPos.y += 0.6f;

        yield return gameObject.transform.DOMove(targetPos, time);

        curTile = candidate[rand];

        CurrentTileUpdate(lastTile);
        CurrentTileUpdate(curTile);

        lastTile = curTile;
    }

    public void CurrentTileUpdate(Tile tile)
    {
        if (tile == null)
            return;

        if (tile == curTile)
        {
            ((GameObject)(tile.GameEntity)).GetComponent<TileBase>().UpdateZombieInfo(this);
        }
        else
        {
            ((GameObject)(tile.GameEntity)).GetComponent<TileBase>().UpdateZombieInfo(null);
        }
    }

    public void SumZombies(ZombieBase zombie)
    {
        zombieData.count += zombie.zombieData.count;
        zombie.zombieData.count = 0;

        ZombieModelChoice(zombieData.count);
        SizeUpCheck();
        CurrentTileUpdate(curTile);
    }

    public int GetRandom()
    {
        float percentage = zombieData.movePossibility + zombieData.stayPossibility;
        float probability = zombieData.movePossibility / percentage;
        float rate = percentage - (percentage * probability);
        int tmp = (int)Random.Range(0, percentage);

        if (tmp <= rate - 1)
        {
            return 0;
        }

        return 1;
    }

    public void AttackPlayer(Player player)
    {
        // 공격 애니메이션
        player.TakeDamage(zombieData.count);
    }

    public void TakeDamage()
    {
        // 피격 애니메이션

        // 사망
        zombieData.count = 0;
        CurrentTileUpdate(null);
        Destroy(this);
        // 시체 오브젝트 생성
    }


    public void Stun(int time = 1)
    {
        remainStunTime = time;
    }
}