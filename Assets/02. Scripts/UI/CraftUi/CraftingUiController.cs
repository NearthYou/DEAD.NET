using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUiController : ControllerBase
{
    [Header ("Craft Mode")]
    [SerializeField] Transform craftSlotParent;
    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject craftSlotPrefab;

    List<ItemBase> craftItems = new List<ItemBase>();
    List<ItemCombineData> itemCombines = new List<ItemCombineData>();

    [Header("Equip Mode")]
    [SerializeField] Transform equipSlotParent;
    [SerializeField] EquipSlot[] equipSlots;

    [Header("Blueprint Mode")]
    [SerializeField] Transform blueprintSlotParent;
    [SerializeField] GameObject blueprintSlotPrefab;

    /// <summary>
    /// ���� ItemSO�� �߰����� ���� ������ ���� �ÿ� ������ �ӽ� ������
    /// </summary>
    [SerializeField] ItemBase tempItem;

    public override EControllerType GetControllerType()
    {
        return EControllerType.CRAFT;
    }





    void Start()
    {
        int i = 1001;
        while(true)
        {
            App.instance.GetDataManager().itemCombineData.TryGetValue(i, out ItemCombineData itemData); //ItemCombineData���� ��� �� itemComines ����Ʈ�� �߰�

            if (itemData == null) break;

            itemCombines.Add(itemData);
            i++;
        }

        InitCraftSlots();
        InitEquipSlots();
        InitBlueprintSlots();
    }





    public void InitCraftSlots()
    {
        for (int i = 0; i < craftSlotParent.childCount; i++)
            Destroy(craftSlotParent.GetChild(i).gameObject);
    }

    void InitEquipSlots()
    {
        foreach (var slot in equipSlots)
            slot.item = null;
    }

    void InitBlueprintSlots()
    {
        for (int i = 0; i < blueprintSlotParent.childCount; i++)
            Destroy(blueprintSlotParent.GetChild(i).gameObject);
    }





    #region Craft
    /// <summary>
    /// CraftBag ���ΰ�ħ(?)
    /// </summary>
    public void UpdateCraft()
    {
        InitCraftSlots();

        bool isFirst = true;
        for(int i = 0; i < craftItems.Count; i++)
        {
            GameObject obj = Instantiate(craftSlotPrefab, craftSlotParent);
            obj.GetComponentInChildren<CraftSlot>().item = craftItems[i];
            if (isFirst == true)
            {
                obj.transform.GetChild(1).gameObject.SetActive(false);
                isFirst = false;
            }
        }
        CompareToCombineData();
    }





    /// <summary>
    /// ����ǥ�� ��
    /// </summary>
    public void CompareToCombineData()
    {
        int flag; // 0: ��ġ, 1: ����ġ

        foreach(ItemCombineData combineData in itemCombines)
        {
            flag = 0;

            string[] combinationCodes = GetCombinationCodes(combineData);
         
            for (int i = 0; i < craftItems.Count; i++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if (combinationCodes[k] == craftItems[i].itemCode)
                    {
                        combinationCodes[k] = "-1";
                        break;  
                    }
                    if (k == 7) flag = 1;
                }
            }

            for (int k = 0; k < 8; k++)
            {
                if (combinationCodes[k] != "-1")
                {
                    flag = 1;
                    break;
                }
            }

            if (flag == 0)
            {
                ItemBase item = GetResultItemByItemCode(combinationCodes[8]);
                AddCombineItem(item);
                break;
            }
        }
    }

    string[] GetCombinationCodes(ItemCombineData _combineData)
    {
        string[] codes = new string[9];

        codes[0] = _combineData.Material_1;
        codes[1] = _combineData.Material_2;
        codes[2] = _combineData.Material_3;
        codes[3] = _combineData.Material_4;
        codes[4] = _combineData.Material_5;
        codes[5] = _combineData.Material_6;
        codes[6] = _combineData.Material_7;
        codes[7] = _combineData.Material_8;
        codes[8] = _combineData.Result;

        return codes;
    }





    /// <summary>
    /// ���� ��� ������ ItemBase���� �˻� �� ����
    /// </summary>
    public ItemBase GetResultItemByItemCode(string _resultItemCode)
    {
        foreach(ItemBase item in itemSO.items)
        {
            if (item.itemCode == _resultItemCode)
                return item;
        }

        Debug.Log("���� �߰����� ���� ������: " + _resultItemCode);
        return tempItem;
    }

    /// <summary>
    /// ���� ��� ������ CraftBag�� ǥ��
    /// </summary>
    /// <param name="_item"></param>
    public void AddCombineItem(ItemBase _item)
    {
        GameObject obj = Instantiate(craftSlotPrefab, craftSlotParent);
        obj.GetComponentInChildren<CraftSlot>().item = _item;
        obj.GetComponentInChildren<CraftSlot>().eSlotType = ESlotType.ResultSlot;
    }





    /// <summary>
    /// CraftBag�� ������ �߰�
    /// </summary>
    /// <param name="_item"></param>
    public void MoveInventoryToCraft(ItemBase _item)
    {
        UIManager.instance.GetInventoryController().RemoveItem(_item);
        craftItems.Add(_item);
        UpdateCraft();
    }

    public void MoveCraftToInventory(ItemBase _item)
    {
        UIManager.instance.GetInventoryController().AddItem(_item);
        craftItems.Remove(_item);
        UpdateCraft();
    }

    public void MoveResultToInventory(ItemBase _item)
    {
        UIManager.instance.GetInventoryController().AddItem(_item);
        craftItems.Clear();
        InitCraftSlots();
    }





    /// <summary>
    /// �ش� �������� Ư�� ������ŭ �ִ��� üũ�ϴ� �Լ�
    /// </summary>
    /// <param name="itemCode"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool CheckCraftingItem(string _itemCode, int _count = 1)
    {
        int cnt = 0;

        for (int i = 0; i < craftItems.Count; i++)
        {
            if (craftItems[i].itemCode == _itemCode)
                cnt++;
        }

            return (cnt == _count);
    }
    #endregion





    #region Equip
    void AddEquip(ItemBase _item)
    {
        for (int i = 0; i < equipSlots.Length; i++) 
        {
            if (equipSlots[i].equipType != _item.data.EquipType)
                continue;

            if (equipSlots[i].item != null)
                UIManager.instance.GetInventoryController().AddItem(equipSlots[i].item);

            equipSlots[i].item = _item;
        }
    }

    void RemoveEquip(ItemBase _item)
    {
        for (int i = 0; i < equipSlots.Length; i++)
        {
            if (equipSlots[i].equipType != _item.data.EquipType)
                continue;

            if (equipSlots[i].item != null)
                UIManager.instance.GetInventoryController().AddItem(equipSlots[i].item);

            equipSlots[i].item = null;
        }
    }

    public void MoveInventoryToEquip(ItemBase _item)
    {
        UIManager.instance.GetInventoryController().RemoveItem(_item);
        AddEquip(_item);
    }

    public void MoveEquipToInventory(ItemBase _item)
    {
        RemoveEquip(_item);
    }
    #endregion





    #region Blueprint
    public void ShowItemBlueprint(ItemBase _item)
    {
        InitBlueprintSlots();

        string[] blueprintCodes = new string[9];

        foreach (ItemCombineData combineData in itemCombines)
        {
            if (combineData.Result == _item.itemCode)
            {
                blueprintCodes = GetCombinationCodes(combineData);
                Debug.Log(_item.itemCode);
                break;
            }
        }

        foreach (string blueprintCode in blueprintCodes)
        {
            if (blueprintCode == null || blueprintCode == "-1") break;
            AddItemByItemCode(blueprintCode);
        }
    }

    void AddItemByItemCode(string _itemCode)
    {
        for (int i = 0; i < itemSO.items.Length; i++)
            if (itemSO.items[i].itemCode == _itemCode)
                AddBlueprintItem(itemSO.items[i]);

        Debug.Log("���� �߰����� ���� ������: " + _itemCode);
        AddBlueprintItem(tempItem);
    }

    void AddBlueprintItem(ItemBase _item)
    {
        GameObject obj = Instantiate(blueprintSlotPrefab, blueprintSlotParent);
        obj.GetComponentInChildren<BlueprintSlot>().item = _item;
        obj.GetComponentInChildren<BlueprintSlot>().enabled = false;
    }
    #endregion





    /// <summary>
    /// Exit ��ư ������ �� �κ��丮�� ������ ��ȯ
    /// </summary>
    public void ExitUi()
    {
        ExitCraftBag();
        ExitEquipBag();
        ExitBlueprintBag();
    }

    public void ExitCraftBag()
    {
        for (int i = 0; i < craftItems.Count; i++)
            UIManager.instance.GetInventoryController().AddItem(craftItems[i]);

        InitCraftSlots();
        craftItems.Clear();
    }

    public void ExitEquipBag()
    {
        for (int i = 0; i < equipSlots.Length; i++) 
        {
            if (equipSlots[i].item == null) continue;
            UIManager.instance.GetInventoryController().AddItem(equipSlots[i].item);
            equipSlots[i].item = null;
        }
    }

    public void ExitBlueprintBag()
    {
        InitBlueprintSlots();
    }
}
