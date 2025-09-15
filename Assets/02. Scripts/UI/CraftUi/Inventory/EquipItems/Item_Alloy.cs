using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ALLOY", menuName = "EquipItems/Item_Alloy")]
public class Item_Alloy : ItemBase
{
    int beforeDurabillity = 0;

    public override void Equip()
    {
        beforeDurabillity = MapController.Instance.Player.Durability;

        MapController.Instance.Player.Durability += (int)data.value1;
        UIManager.instance.GetUpperController().IncreaseDurabillityAnimation();
    }

    public override bool CheckMeetCondition()
    {
        return (MapController.Instance.Player.Durability <= beforeDurabillity);
    }
}
