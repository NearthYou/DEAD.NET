using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "SHAPEMETAL", menuName = "EquipItems/Item_Shapemetal")]
public class Item_Shapemetal : ItemBase
{
    int beforeDay = 0;
    int beforeDurabillity = 0;

    public override void Equip()
    {
        beforeDay = UIManager.instance.GetNoteController().dayCount;
        beforeDurabillity = MapController.Instance.Player.Durability;

        MapController.Instance.Player.Durability += (int)data.value1;
        MapController.Instance.Player.ClockUntil((int)data.value2);
        UIManager.instance.GetUpperController().IncreaseDurabillityAnimation();
    }

    public override bool CheckMeetCondition()
    {
        return (UIManager.instance.GetNoteController().dayCount - beforeDay >= 6 && MapController.Instance.Player.Durability <= beforeDurabillity) ;
    }
}
