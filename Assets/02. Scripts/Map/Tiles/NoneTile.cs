using System.Collections;
using System.Collections.Generic;
using Hexamap;
using UnityEngine;

public class NoneTile : TileBase, ITileLandformEffect
{
    public void Buff(Player _player)
    {
        _player.ChangeMoveRange(this.GetComponent<TileBase>().TileType);
    }

    public void DeBuff(Player _player)
    {
        if (RandomPercent.GetRandom(30))
        {
            _player.ChangeDurbility(-3);
        }
       
        if (RandomPercent.GetRandom(5))
        {
            UIManager.instance.GetInventoryController().RemoveRandomItem();
        }
    }
}
