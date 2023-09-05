using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUiController : ControllerBase
{
    [SerializeField] Transform slotParent;
    [SerializeField] Sprite[] craftTypeImage;
    [SerializeField] ItemSO itemSO;
    [SerializeField] GameObject slotPrefab;

    List<ItemBase> items;
    List<ItemCombineData> itemCombines;

    Transform firstChild;
    Transform lastChild;

    string[] combinationCodes = new string[9];

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
        items = new List<ItemBase>();
        itemCombines = new List<ItemCombineData>();

        int i = 1001;
        while(true)
        {
            App.instance.GetDataManager().itemCombineData.TryGetValue(i, out ItemCombineData itemData); //ItemCombineData���� ��� �� itemComines ����Ʈ�� �߰�

            if (itemData == null) break;

            itemCombines.Add(itemData);
            i++;
        }

        InitSlots();
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

    public void InitSlots()
    {
        for(int i = 0; i < slotParent.childCount; i++)
        {
            Destroy(slotParent.GetChild(i).gameObject);
        }
    }

    /// <summary>
    /// CraftBag ���ΰ�ħ(?)
    /// </summary>
    public void FreshCraftingBag()
    {
        InitSlots();

        for(int i = 0; i < items.Count; i++)
        {
            GameObject obj = Instantiate(slotPrefab, slotParent);
            obj.GetComponentInChildren<ItemSlot>().item = items[i];
        }

        SetFirstAndLastSlot();
    }

    /// <summary>
    /// CraftBag�� ������ �߰�
    /// </summary>
    /// <param name="_item"></param>
    public void CraftItem(ItemBase _item)
    {
        items.Add(_item);
        FreshCraftingBag();
        CompareToCombineData();
    }

    void SetFirstAndLastSlot()
    {
        firstChild = slotParent.GetChild(0);
        lastChild = slotParent.GetChild(slotParent.childCount - 1);
        firstChild.GetChild(1).gameObject.SetActive(false);
    }
    /// <summary>
    /// Exit ��ư ������ �� �κ��丮�� ������ ��ȯ
    /// </summary>
    public void ExitUi()
    {
        for (int i = 0; i < items.Count; i++)
        {
            UIManager.instance.GetInventoryController().AddItem(items[i]);
        }

        InitSlots();
        items.Clear();
    }

    /// <summary>
    /// ����ǥ ��
    /// </summary>
    public void CompareToCombineData()
    {
        int flag; // 0: ��ġ, 1: ����ġ

        foreach(ItemCombineData combineData in itemCombines)
        {
            flag = 0;

            GetCombinationCodes(combineData);

            for (int i = 0; i < items.Count; i++)
            {
                for (int k = 0; k < 8; k++)
                {
                    if (combinationCodes[k] == "1" || combinationCodes[k] == "-1") continue;
                    if (combinationCodes[k] == items[i].itemCode)
                    {
                        combinationCodes[k] = "1";
                        break;
                    }
                }
            }

            for (int k = 0; k < 8; k++)
            {
                if (combinationCodes[k] == "1") continue;
                else
                {
                    flag = 1; break;
                }
            }

            if (flag == 0)
            {
                ItemBase item = GetResultItem(combinationCodes[8]);
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
    public ItemBase GetResultItem(string resultItemCode)
    {
        ItemBase resultItem;

        for (int i = 0; i < itemSO.items.Length; i++)
        {
            if (itemSO.items[i].itemCode == resultItemCode)
            {
                resultItem = itemSO.items[i];
                App.instance.GetDataManager().itemData.TryGetValue(resultItemCode, out ItemData itemData);
                resultItem.data = itemData;
                
                return resultItem;
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
        GameObject obj = Instantiate(slotPrefab, slotParent);
        obj.GetComponentInChildren<ItemSlot>().item = _item;
        obj.GetComponentInChildren<ItemSlot>().eSlotType = ESlotType.ResultSlot;
        obj.GetComponent<Image>().sprite = craftTypeImage[1];
        SetFirstAndLastSlot();
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

        for (int i = 0; i < items.Count; i++)
        {
            if (items[i].itemCode == itemCode)
                _cnt++;
        }

        if (_cnt == count)
            return true;
        else
            return false;
    }

    public void CraftToInventory(ItemBase _item)
    {
        items.Remove(_item);
        FreshCraftingBag();
        CompareToCombineData();
    }

    public void ResultToInventory()
    {
        UIManager.instance.GetInventoryController().AddItem(lastChild.GetComponentInChildren<ItemSlot>().item);
        items.Clear();
        InitSlots();
    }
}
