using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Hexamap;
using DG.Tweening;
using FischlWorks_FogWar;
using Yarn.Unity;
using Random = UnityEngine.Random;

public class Player : MonoBehaviour
{
    [Header("Components")]
    [SerializeField] private FloatingEffect floating;
    
    [Header("Settings")]
    private int maxMoveRange = 1;
    private int durability;
    private int moveRange;
    private int temporaryRange;
    private int temporaryDurability;
    
    [Header("State")]
    private List<Coords> movePath;
    private TileController currentTileContorller;
    private bool isDead;
    private bool isClockingCheck;
    private bool isJungleDebuff;
    private int moveBuffDuration;
    private int durabilityBuffDuration;
    private int clockBuffDuration;
    
    [Header("Materials")]
    [SerializeField] private Renderer rend;
    [SerializeField] private Material cloakingMaterial;
    [SerializeField] private Material normalMaterial;

    public static Action PlayerSightUpdate;
    
    public int Durability
    {
        get => durability;
        set
        {
            if (value > 0)
                durability = value;
        }
    }

    public int MoveRange
    {
        get => moveRange;
        set => moveRange = value;
    }

    public List<Coords> MovePath
    {
        get => movePath;
        set => movePath = value;
    }

    public TileController TileController
    {
        get => currentTileContorller;
        set => currentTileContorller = value;
    }

    public bool JungleDebuff => isJungleDebuff;

    private void Start()
    {
        movePath = new List<Coords>();
        moveRange = maxMoveRange;
        clockBuffDuration = 0;
        StartCoroutine(DelaySightGetInfo());
    }

    public void InputDefaultData(int _moveRange, int _durability)
    {
        maxMoveRange = _moveRange;
        moveRange = maxMoveRange;
        durability = _durability;
    }

    public IEnumerator ActionDecision(TileController targetTileController)
    {
        if (JungleDebuff)
        {
            // 이동
            yield return StartCoroutine(MoveToRandom());
            
            isJungleDebuff = false;
        }
        else
        {
            // 공격
            yield return StartCoroutine(MoveToTarget(targetTileController));
        }
    }
    
    public IEnumerator MoveToTarget(TileController targetTileController, float time = 0.4f)
    {
        Tile targetTile;
        Vector3 targetPos;
        Vector3 lastTargetPos = targetTileController.transform.position;

        var zombies = targetTileController.GetComponent<TileBase>().CurZombies;

        // 이동한 타일에 좀비가 있다면 공격
        if (zombies != null)
        {
            AttackZombies(zombies);
        }
        else
        {
            // 이동한 타일에 좀비가 없다면 이동
            for (int i = 0; i < movePath.Count; i++)
            {
                Coords coords = movePath[i];
                targetTile = MapController.instance.GetTileFromCoords(coords);

                if (targetTile == null)
                    break;

                targetPos = ((GameObject)targetTile.GameEntity).transform.position;
                targetPos.y += 0.5f;

                transform.DOMove(targetPos, time);
                moveRange--;
                yield return new WaitForSeconds(time);
            }

            lastTargetPos.y += 0.5f;
            yield return transform.DOMove(lastTargetPos, time);
            yield return new WaitForSeconds(time);
        }

        movePath.Clear();
        moveRange = 0;

        UpdateCurrentTile(targetTileController);
    }

    public IEnumerator MoveToRandom(int num = 1, float time = 0.25f)
    {
        var candidate = MapController.instance.GetTilesInRange(currentTileContorller.Model, num);

        Vector3 targetPos = currentTileContorller.transform.position;
        Tile tile = candidate[0];
        bool isFindPath = false;
        
        for (int i = 0; i < candidate.Count; i++)
        {
            if(App.instance.GetMapManager().mapController.CheckTileType(candidate[i], "LandformPlain"))
            {
                if (((GameObject)candidate[i].GameEntity).GetComponent<TileBase>().Structure == null &&
                    ((GameObject)candidate[i].GameEntity).GetComponent<TileBase>().CurZombies == null)
                {
                    targetPos = ((GameObject)candidate[i].GameEntity).transform.position;
                    tile = candidate[i];
                    isFindPath = true;
                    break;
                }
            }
        }
        
        targetPos.y += 0.6f;

        yield return gameObject.transform.DOMove(targetPos, time);

        movePath.Clear();
        moveRange = 0;

        if(isFindPath)
            UpdateCurrentTile(((GameObject)tile.GameEntity).GetComponent<TileController>());
    }
    
    /// <summary>
    /// 플레이어가 서 있는 타일의 위치를 갱신할 때마다 그 타일의 정보를 넘겨주는 이벤트 함수
    /// </summary>
    /// <returns></returns>
    IEnumerator DelaySightGetInfo()
    {
        // AdditiveScene 딜레이 
        yield return new WaitUntil(() => PlayerSightUpdate != null);
        PlayerSightUpdate?.Invoke();
    }

    public void UpdateCurrentTile(TileController tileController)
    {
        currentTileContorller = tileController;
        PlayerSightUpdate?.Invoke();
    }

    public void UpdateMovePath(List<Coords> path)
    {
        movePath = path;
    }

    public void SetHealth(bool isMax, int num = 0)
    {
        if (isMax)
            moveRange = maxMoveRange;
        else
        {
            moveRange = num;
        }
    }

    public void StartFloatingAnimation()
    {
        StartCoroutine(floating.FloatingAnimation());
    }

    public void AttackZombies(ZombieBase zombies)
    {
        // if (bulletsNum <= 0)
        // {
        //     // 탄 없을 시 행동 불가
        //     return;
        // }

        // 공격 애니메이션
        zombies.TakeDamage();

        //bulletsNum -= 1;
    }

    public void TakeDamage(int zombieCount)
    {
        if (isDead)
            return;

        // 피격 애니메이션
        if (durability - zombieCount > 0)
        {
            durability -= zombieCount;
            UIManager.instance.GetNextDayController().isHit = true;
        }
        else if (durability - zombieCount <= 0)
        {
            // 내구도가 0이 되면 게임 오버
            durability = 0;
            isDead = true;
            UIManager.instance.GetNextDayController().isHit = true;

            // 게임 오버
            UIManager.instance.GetNextDayController().isOver = true;
        }
    }

    public void ChangeMoveRange(ETileType _type)
    {
        switch (_type)
        {
            case ETileType.None:
                moveRange += 1;
                break;
            case ETileType.Desert:
                moveRange = 0;
                break;
            default:
                break;
        }
    }

    public void ChangeDurbility(int amount)
    {
        if (durability + amount > 0)
            durability += amount;
        
        //UIManager.instance.GetUpperController().UpdateDurabillity(); 
    }

    public void AddMoveRangeUntil(int _duration, int _amount)
    {
        moveBuffDuration = _duration;
        temporaryRange = maxMoveRange + _amount;
    }
    
    public void ClockUntil(int _duration)
    {
        clockBuffDuration = _duration;
        rend.material = cloakingMaterial;
    }
    
    public void AddDurability(int _durability, int _amount)
    {
        durabilityBuffDuration = _durability;
        temporaryDurability = durability + _amount;
    }
    
    public bool GetIsClocking()
    {
        if (clockBuffDuration > 0)
        {
            return true;
        }
        else
        {
            clockBuffDuration = 0;
            rend.material = normalMaterial;
            return false;
        }
    }

    public void ChangeClockBuffDuration()
    {
        if (clockBuffDuration > 0) 
            clockBuffDuration--;
    }


    public void AddSightRange(int _amount)
    {
        csFogWar.instance.AddSightRange(_amount);
    }
    
    public void TileEffectCheck()
    {
        var tileBase = currentTileContorller.GetComponent<TileBase>();

        switch (tileBase.TileType)
        {
            case ETileType.None:
                (tileBase as NoneTile).Buff(this);
                (tileBase as NoneTile).DeBuff(this);
                break;
            case ETileType.Desert:
                (tileBase as DesertTile).DeBuff(this);
                break;
            case ETileType.Tundra:
                (tileBase as TundraTile).DeBuff(this);
                break;
            case ETileType.Jungle:
                (tileBase as JungleTile).DeBuff(this);
                break;
            default:
                break;
        }
    }

    public void JungleDebuffOn()
    {
        isJungleDebuff = true;
    }
}