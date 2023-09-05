using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryController : ControllerBase
{
    [SerializeField] Transform slotParent;
    [SerializeField] Sprite[] itemTypeImage;
    [SerializeField] ItemSO itemSO;

    private ItemSlot[] slots;
    private Temp[] slotImages;
    private TextMeshProUGUI[] itemCounts;

    public List<ItemBase> items;

    int slotCount = 0;

    public override EControllerType GetControllerType()
    {
        return EControllerType.INVENTORY;
    }

    private void OnValidate()
    {
        slots = slotParent.GetComponentsInChildren<ItemSlot>();
        slotImages = slotParent.GetComponentsInChildren<Temp>();
        itemCounts = slotParent.GetComponentsInChildren<TextMeshProUGUI>();
    }

    void Awake()
    {
        AddItemByCode("ITEM_TIER_2_SIGNALLER");
        AddItemByCode("ITEM_TIER_2_PLASMA");
        AddItemByCode("ITEM_TIER_1_PLASTIC");
        AddItemByCode("ITEM_TIER_1_STEEL");
        FreshSlot();
    }

    void Start()
    {
        FreshSlot();
    }

    /// <summary>
    /// slot�� ������� ���� �� ȣ���. �κ��丮 ���� ���Կ� ������ �߰�
    /// </summary>
    public void FreshSlot()
    {
        ClearSlots();

        slotCount = 0;

        for (int i = 0; i < items.Count; i++)
        {
            int flag = 0;

            for (int j = 0; j < slotCount; j++)
            {
                if (slots[j].item == items[i])
                {
                    items[i].itemCount++;
                    itemCounts[j].text = items[i].itemCount.ToString();
                    flag = 1;
                    break;
                }
            }

            if (flag == 1)
                continue;

            slots[slotCount].item = items[i];
            slots[slotCount].item.itemCount = 1;
            itemCounts[slotCount].gameObject.SetActive(true);
            itemCounts[slotCount].text = items[i].itemCount.ToString();

            if (items[i].itemType == ItemType.Consumption)
                slotImages[slotCount].GetComponent<Image>().sprite = itemTypeImage[0];
            else if (items[i].itemType == ItemType.Equipment)
                slotImages[slotCount].GetComponent<Image>().sprite = itemTypeImage[1];
            else if (items[i].itemType == ItemType.Material)
                slotImages[slotCount].GetComponent<Image>().sprite = itemTypeImage[2];

            if (slots[slotCount].item != null)
                slotCount++;
        }
    }

    /// <summary>
    /// slot �ʱ�ȭ
    /// </summary>
    private void ClearSlots()
    {
        for (int i = 0; i < slotParent.childCount; i++)
        {
            slots[i].item = null;
            slotImages[i].GetComponent<Image>().sprite = itemTypeImage[0];
            itemCounts[i].text = "0";
            itemCounts[i].gameObject.SetActive(false);
        }

        slotCount = 0;
    }

    /// <summary>
    /// �κ��丮�� ItemBase�� �̿��Ͽ� ������ �߰�
    /// </summary>
    /// <param name="_item"></param>
    public void AddItem(ItemBase _item)
    {
        if (slotCount < slots.Length)
        {
            string code = _item.itemCode;
            App.instance.GetDataManager().itemData.TryGetValue(code, out ItemData itemData);
            _item.data = itemData;
            items.Add(_item);
            FreshSlot();
        }
        else
        {
            Debug.Log("������ ���� �� �ֽ��ϴ�.");
        }
    }

    /// <summary>
    /// �κ��丮�� itemCode�� �̿��Ͽ� ������ �߰�
    /// </summary>
    /// <param name="itemCode"></param>
    public void AddItemByCode(string itemCode)
    {
        ItemBase item = null;

        for (int i = 0; i < itemSO.items.Length; i++)
        {
            if (itemSO.items[i].itemCode == itemCode)
            {
                item = itemSO.items[i];
                App.instance.GetDataManager().itemData.TryGetValue(itemCode, out ItemData itemData);
                item.data = itemData;
                items.Add(item);
            }
        }
    }

    /// <summary>
    /// �κ��丮���� ������ ����
    /// </summary>
    /// <param name="_item"></param>
    public void RemoveItem(ItemBase _item)
    {
        _item.itemCount--;
        for(int i = 0; i < items.Count; i++)
        {
            if (items[i] == _item)
            {
                items.RemoveAt(i);
                break;
            }
        }
        FreshSlot();
    }

    /// <summary>
    /// �κ��丮 ���� Ư�� ������ �����ϴ��� üũ
    /// </summary>
    /// <param name="itemCode"></param>
    /// <returns></returns>
    public bool CheckInventoryItem(string itemCode)
    {
        for(int i = 0; i < items.Count; i++)
        {
            if (items[i].itemCode == itemCode)
                return true;
        }

        return false;
    }
}
