using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BLADE", menuName = "EquipItems/Item_Blade")]
public class Item_Blade : ItemBase
{
    int beforeDurabillity = 0;

    public override void Equip()
    {
        beforeDurabillity = App.instance.GetMapManager().mapController.Player.Durability;
        App.instance.GetMapManager().mapController.Player.Durability += (int)data.value1;
        UIManager.instance.GetUpperController().IncreaseDurabillityAnimation();
    }

    public override bool CheckMeetCondition()
    {
        return (App.instance.GetMapManager().mapController.Player.Durability <= beforeDurabillity);
    }
}
