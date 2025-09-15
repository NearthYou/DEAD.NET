using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ArmyStructure : StructureBase
{
    public override void Init(List<TileBase> _neighborTiles, GameObject _structureModel, ItemSO _itemSO)
    {
        structureName = "군사 시설";

        var itemBase = _itemSO.items.ToList()
            .Find(x => x.data.Code == "ITEM_DISTURBE");

        resource = new Resource(itemBase.English, 2, itemBase);
        isUse = false;
        isAccessible = false;

        neighborTiles = _neighborTiles;
        structureModel = _structureModel;
    }


    public override void NoFunc()
    {
        isUse = true;
    }

    public override void YesFunc()
    {
        for (var index = 0; index < colleagues.Count; index++)
        {
            var tile = colleagues[index];
            ((ArmyStructure)tile.Structure).AllowAccess();
        }

        App.instance.GetMapManager().NormalStructureResearch(this);

        isUse = true;
        isAccessible = true;

        UIManager.instance.GetPageController().CreateSelectDialogueRunner("sequence");
    }
}