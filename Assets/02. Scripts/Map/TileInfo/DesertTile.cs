using System.Collections;
using System.Collections.Generic;
using Hexamap;
using UnityEngine;

public class DesertTile : TileBase, ITileLandformEffect
{
    public void Buff(Player player)
    {
        throw new System.NotImplementedException();
    }

    public void DeBuff(Player _player)
    {
        _player.ChangeDurbility(-1);
        
        if(RandomPercent.GetRandom(5))
            _player.SetHealth(false);
    }
}
