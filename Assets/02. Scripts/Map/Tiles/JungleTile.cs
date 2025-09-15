using System.Collections;
using System.Collections.Generic;
using Hexamap;
using UnityEngine;

public class JungleTile : TileBase, ITileLandformEffect
{
    public void Buff(Player player)
    {
        throw new System.NotImplementedException();
    }

    public void DeBuff(Player _player)
    {
        if (RandomPercent.GetRandom(30))
        {
            UIManager.instance.GetPageController().SetResultPage("LOOSE_WAY", false);
            _player.JungleDebuffOn();
        }

        if (RandomPercent.GetRandom(10))
        {
            UIManager.instance.GetPageController().SetResultPage("SWAMP", false);
            _player.SetHealth(false);
        }
    }
}