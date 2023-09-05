using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUiController : ControllerBase
{
    [SerializeField] Transform slotParent;
    [SerializeField] Sprite[] craftTypeImage;
    [SerializeField] ItemSO itemSO;

    ItemSlot[] slots;

    List<Transform> slotTransforms;
    List<Image> craftTypeImages;
    List<ItemBase> items;
    List<ItemCombineData> itemCombines;

    string[] combinationCodes = new string[9];

    /// <summary>
    /// ���� ItemSO�� �߰����� ���� ������ ���� �ÿ� ������ �ӽ� ������
    /// </summary>
    [SerializeField] ItemBase tempItem;

    public override EControllerType GetControllerType()
    {
        return EControllerType.CRAFT;
    }

    void Awake()
    {
        slots = slotParent.GetComponentsInChildren<ItemSlot>();
        slotTransforms = new List<Transform>();
        craftTypeImages = new List<Image>();

        foreach (Transform child in slotParent)
        {
            slotTransforms.Add(child);
            craftTypeImages.Add(child.GetChild(1).GetComponent<Image>());
        }
    }

    void Start()
    {
        itemCombines = new List<ItemCombineData>();

        for (int i = 1001; i < 2000; i++)
        {
            App.instance.GetDataManager().itemCombineData.TryGetValue(i, out ItemCombineData itemData); //ItemCombineData���� ��� �� itemComines ����Ʈ�� �߰�

            if (itemData != null)
                itemCombines.Add(itemData);
            else
                break;
        }

        for (int i = 0; i < slots.Length; i++) //CraftBag ���� slot ��� �ʱ�ȭ
        {
            slotTransforms[i].gameObject.SetActive(false);
            slots[i].item = null;
            slots[i].eSlotType = ESlotType.CraftingSlot;
            craftTypeImages[i].GetComponent<Image>().sprite = craftTypeImage[0];
        }
    }

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

    /// <summary>
    /// CraftBag ���ΰ�ħ(?)
    /// </summary>
    public void FreshCraftingBag()
    {
        for (int i = 0; i < items.Count + 1; i++)
        {
            slotTransforms[i].gameObject.SetActive(false);
            slots[i].item = null;
            slots[i].eSlotType = ESlotType.CraftingSlot;
            craftTypeImages[i].GetComponent<Image>().sprite = craftTypeImage[0];
        }

        int j = 0;

        craftTypeImages[j].gameObject.SetActive(false);

        for (; j < items.Count; j++)
        {
            slotTransforms[j].gameObject.SetActive(true);
            slots[j].item = items[j];
        }

        for (; j < slots.Length; j++)
        {
            slotTransforms[j].gameObject.SetActive(false);
            slots[j].item = null;
        }
    }

    /// <summary>
    /// CraftBag�� ������ �߰�
    /// </summary>
    /// <param name="_item"></param>
    public void CraftItem(ItemBase _item)
    {
        items.Add(_item);
        FreshCraftingBag();
        CombineItem();
        
    }

    /// <summary>
    /// Exit ��ư ������ �� �κ��丮�� ������ ��ȯ
    /// </summary>
    public void ReturnItem()
    {
        for (int i = 0; i < items.Count; i++)
        {
            UIManager.instance.GetInventoryController().AddItem(items[i]);
            slotTransforms[i].gameObject.SetActive(false);
            slots[i].item = null;
        }

        items.Clear();
    }

    /// <summary>
    /// ����ǥ ��
    /// </summary>
    public void CombineItem()
    {
        int flag; // 0: ��ġ, 1: ����ġ

        foreach(ItemCombineData combineData in itemCombines)
        {
            flag = 0;

            combinationCodes[0] = combineData.Material_1;
            combinationCodes[1] = combineData.Material_2;
            combinationCodes[2] = combineData.Material_3;
            combinationCodes[3] = combineData.Material_4;
            combinationCodes[4] = combineData.Material_5;
            combinationCodes[5] = combineData.Material_6;
            combinationCodes[6] = combineData.Material_7;
            combinationCodes[7] = combineData.Material_8;
            combinationCodes[8] = combineData.Result;

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
                if (combinationCodes[k] == "1" || combinationCodes[k] == "-1") continue;
                else
                {
                    flag = 1; break;
                }
            }

            if (flag == 0)
            {
                //if (combinationCodes[8] == "ITEM_TIER_2_SIGNALLER" || combinationCodes[8] == "ITEM_TIER_2_RISISTOR") continue;
                Debug.Log(combinationCodes[8]);
                ItemBase item = CombineResultItem(combinationCodes[8]);
                AddCombineItem(item);
                break;
            }
        }
    }

    /// <summary>
    /// ���� ��� ������ ItemBase���� �˻� �� ����
    /// </summary>
    public ItemBase CombineResultItem(string resultItemCode)
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
        int slotIndex = items.Count;
        slotTransforms[slotIndex].gameObject.SetActive(true);
        slots[slotIndex].item = _item;
        craftTypeImages[slotIndex].GetComponent<Image>().sprite = craftTypeImage[1];
        slots[slotIndex].eSlotType = ESlotType.ResultSlot;
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

    public void CraftToInventory(ItemSlot itemSlot)
    {
        items.Remove(itemSlot.item);
        UIManager.instance.GetInventoryController().AddItem(slots[items.Count].item);
        FreshCraftingBag();
        CombineItem();
    }

    public void ResultToInventory()
    {
        UIManager.instance.GetInventoryController().AddItem(slots[items.Count].item);
        slotTransforms[items.Count].gameObject.SetActive(false);
        slots[items.Count].item = null;
        items.Clear();
        FreshCraftingBag();
        CombineItem();
    }
}
