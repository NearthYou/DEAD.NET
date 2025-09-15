using System.Collections;
using System.Collections.Generic;
using Hexamap;
using UnityEngine;

public class TundraTile : TileBase, ITileLandformEffect
{
    public void Buff(Player player)
    {
        throw new System.NotImplementedException();
    }

    public void DeBuff(Player _player)
    {
        App.instance.GetMapManager().TundraTileCheck();

        if (RandomPercent.GetRandom(10))
        {
            App.instance.GetMapManager().EtherResourceCheck();
        }
    }
}