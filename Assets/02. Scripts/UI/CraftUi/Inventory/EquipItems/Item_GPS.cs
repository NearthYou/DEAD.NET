using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "GPS", menuName = "EquipItems/Item_GPS")]
public class Item_GPS : ItemBase
{
    int beforeDay = 0;

    public override void Equip()
    {
        beforeDay = UIManager.instance.GetNoteController().dayCount;

        MapController.Instance.Player.AddSightRange((int)data.value1);
    }

    public override bool CheckMeetCondition()
    {
        return (UIManager.instance.GetNoteController().dayCount - beforeDay == 1);
    }
}