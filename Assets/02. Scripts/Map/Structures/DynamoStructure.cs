using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class DynamoStructure : StructureBase
{
    public override void Init(List<TileBase> _neighborTiles, GameObject _structureModel, ItemSO _itemSO)
    {
        structureName = "요새";

        var itemBase = _itemSO.items.ToList()
            .Find(x => x.data.Code == "ITEM_DISTURBE");

        resource = new Resource(itemBase.English, 2, itemBase);
        isUse = true;
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
            ((DynamoStructure)tile.Structure).AllowAccess();
        }

        App.instance.GetMapManager().NormalStructureResearch(this);

        isUse = true;
        isAccessible = true;

        UIManager.instance.GetPageController().CreateSelectDialogueRunner("sequence");
    }
}