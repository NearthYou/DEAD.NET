using System.Collections;
using System.Collections.Generic;
using Hexamap;
using UnityEngine;

public class NoneTile : TileBase, ITileLandformEffect
{
    public void Buff(Player _player)
    {
        // 포장된 도로 : 확정 이동 거리 1추가
        _player.ChangeMoveRange(this.GetComponent<TileBase>().TileType);
    }

    public void DeBuff(Player _player)
    {
        // 확률로 구현
        // 무너진 건물사고 : 선체 내구도 -3 감소
        if (RandomPercent.GetRandom(30))
        {
            Debug.Log("무너진 건물 사고 디버프");
            _player.ChangeDurbility(-3);
        }
       
        // 도둑맞은 자원 : 가진 자원 중 1개 랜덤으로 빼앗김
        if (RandomPercent.GetRandom(5))
        {
            Debug.Log("도둑 맞은 자원 디버프");
            UIManager.instance.GetInventoryController().RemoveRandomItem();
        }
    }
}
