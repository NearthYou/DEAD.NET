using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using System.Linq;

public class ProductionStructure : StructureBase
{
    public override void Init(List<TileBase> _neighborTiles, GameObject _structureModel, ItemSO _itemSO)
    {
        structureName = "생산 건물";
        isUse = false;
        isAccessible = false;

        var itemBase = _itemSO.items.ToList()
            .Find(x => x.data.Code == "ITEM_WIRE");

        resource = new Resource(itemBase.data.English, 2, itemBase);
        neighborTiles = _neighborTiles;
        structureModel = _structureModel;

        App.instance.GetDataManager().itemData.TryGetValue("ITEM_NETWORKCHIP", out ItemData itemData);
        specialItem = itemData;
    }

    public override void YesFunc()
    {
        for (var index = 0; index < colleagues.Count; index++)
        {
            var tile = colleagues[index];
            ((ProductionStructure)tile.Structure).AllowAccess();
        }

        App.instance.GetMapManager().NormalStructureResearch(this);
        isUse = true;
        isAccessible = true;
        UIManager.instance.GetPageController().CreateSelectDialogueRunner("sequence");
        UIManager.instance.GetPageController().isClickYesBtnInProductionStructure = true;
    }

    public override void NoFunc()
    {
        // 접근 불가 장애물 타일로 변경
        isUse = true;
    }
}