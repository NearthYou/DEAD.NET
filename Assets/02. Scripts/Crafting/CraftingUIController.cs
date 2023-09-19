using JetBrains.Annotations;
using System;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Progress;

public class CraftingUiController : ControllerBase
{
    [Header ("Craft Mode")]
    [SerializeField] Transform craftSlotParent;
    [SerializeField] Sprite[] craftTypeImage;
    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject craftSlotPrefab;

    List<ItemBase> craftItems;
    List<ItemCombineData> itemCombines;

    string[] combinationCodes = new string[9];

    /// <summary>
    /// ���� ItemSO�� �߰����� ���� ������ ���� �ÿ� ������ �ӽ� ������
    /// </summary>
    [SerializeField] ItemBase tempItem;

    [Header("Equip Mode")]
    [SerializeField] Transform equipSlotParent;
    EquipSlot[] equipSlots;
    public List<ItemBase> equipItems;





    public override EControllerType GetControllerType()
    {
        return EControllerType.CRAFT;
    }





    void Start()
    {
        craftItems = new List<ItemBase>();
        equipItems = new List<ItemBase>();
        itemCombines = new List<ItemCombineData>();

        int i = 1001;
        while(true)
        {
            App.instance.GetDataManager().itemCombineData.TryGetValue(i, out ItemCombineData itemData); //ItemCombineData���� ��� �� itemComines ����Ʈ�� �߰�

            if (itemData == null) break;

            itemCombines.Add(itemData);
            i++;
        }

        equipSlots = equipSlotParent.GetComponentsInChildren<EquipSlot>();

        InitCraftSlots();
        InitEquipSlots();
    }





    #region Craft
    public void InitCraftSlots()
    {
        for(int i = 0; i < craftSlotParent.childCount; i++)
        {
            Destroy(craftSlotParent.GetChild(i).gameObject);
        }
    }





    #region temp
    /// <summary>
    /// �ÿ�ȸ�� �ӽ� �Լ�(�³�?)
    /// </summary>
    void Update()
    {
        InputKey();
    }

    /// <summary>
    /// �������� ������ �Լ��� �ƴմϴ�.. PŰ�� ������ �������� �߰��Ǵ°ǰ� ���׿�~
    /// </summary>
    private void InputKey()
    {
        if (Input.GetKeyDown(KeyCode.P))
        {
            UIManager.instance.GetInventoryController().AddItem(itemSO.items[0]);
            UIManager.instance.GetInventoryController().AddItem(itemSO.items[1]);
            UIManager.instance.GetInventoryController().AddItem(itemSO.items[4]);
        }
    }
    #endregion





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
            if (isFirst)
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

            GetCombinationCodes(combineData);

            for (int i = 0; i < craftItems.Count; i++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if (combinationCodes[k] == "1" || combinationCodes[k] == "-1") continue;
                    if (combinationCodes[k] == craftItems[i].itemCode)
                    {
                        combinationCodes[k] = "1";
                        break;
                    }
                }
            }

            for (int k = 0; k < 8; k++)
            {
                if (combinationCodes[k] == "1" || combinationCodes[k] == "-1") continue;
                else
                {
                    flag = 1; break;
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

    void GetCombinationCodes(ItemCombineData combineData)
    {
        combinationCodes[0] = combineData.Material_1;
        combinationCodes[1] = combineData.Material_2;
        combinationCodes[2] = combineData.Material_3;
        combinationCodes[3] = combineData.Material_4;
        combinationCodes[4] = combineData.Material_5;
        combinationCodes[5] = combineData.Material_6;
        combinationCodes[6] = combineData.Material_7;
        combinationCodes[7] = combineData.Material_8;
        combinationCodes[8] = combineData.Result;
    }





    /// <summary>
    /// ���� ��� ������ ItemBase���� �˻� �� ����
    /// </summary>
    public ItemBase GetResultItemByItemCode(string resultItemCode)
    {
        foreach(ItemBase item in itemSO.items)
        {
            if (item.itemCode == resultItemCode)
            {
                return item;
            }
        }

        Debug.Log("���� �߰����� ���� ������");
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
        obj.transform.GetChild(1).GetComponent<Image>().sprite = craftTypeImage[1];
    }





    /// <summary>
    /// CraftBag�� ������ �߰�
    /// </summary>
    /// <param name="_item"></param>
    public void MoveInventoryToCraft(ItemBase _item)
    {
        craftItems.Add(_item);
        UpdateCraft();
    }

    public void MoveCraftToInventory(ItemBase _item)
    {
        craftItems.Remove(_item);
        UpdateCraft();
    }

    public void MoveResultToInventory()
    {
        craftItems.Clear();
        InitCraftSlots();
    }





    /// <summary>
    /// �ش� �������� Ư�� ������ŭ �ִ��� üũ�ϴ� �Լ�
    /// </summary>
    /// <param name="itemCode"></param>
    /// <param name="count"></param>
    /// <returns></returns>
    public bool CheckCraftingItem(string itemCode, int count = 1)
    {
        int _cnt = 0;

        for (int i = 0; i < craftItems.Count; i++)
        {
            if (craftItems[i].itemCode == itemCode)
                _cnt++;
        }

        if (_cnt == count)
            return true;
        else
            return false;
    }
    #endregion





    #region Equip
    void InitEquipSlots()
    {
        foreach (var slot in equipSlots) 
        {
            slot.item = null;
        }
    }

    void UpdateEquip()
    {
        if (equipItems.Count == 4)
        {
            UIManager.instance.GetInventoryController().AddItem(equipSlots[0].item);
            equipItems.RemoveAt(0);
        }

        InitEquipSlots();

        for (int i = 0; i < equipItems.Count; i++) 
        {
            equipSlots[i].item = equipItems[i];
        }
    }

    public void MoveInventoryToEquip(ItemBase _item)
    {
        equipItems.Add(_item);
        UpdateEquip();
    }

    public void MoveEquipToInventory(ItemBase _item)
    {
        equipItems.Remove(_item);
        UpdateEquip();
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
        {
            UIManager.instance.GetInventoryController().AddItem(craftItems[i]);
        }

        InitCraftSlots();
        craftItems.Clear();
    }

    public void ExitEquipBag()
    {
        foreach (var slot in equipSlots) 
        {
            if (slot.item == null) continue;
            UIManager.instance.GetInventoryController().AddItem(slot.item);
            slot.item = null;
        }
    }

    public void ExitBlueprintBag()
    {

    }
}
