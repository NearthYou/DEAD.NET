using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class InventoryController : ControllerBase
{
    [SerializeField] Transform slotParent;
    [SerializeField] ItemSO itemSO;

    List<List<ItemSlot>> slots = new List<List<ItemSlot>>();
    int[] counts = new int[6];
    public List<ItemBase> items;

    public override EControllerType GetControllerType()
    {
        return EControllerType.INVENTORY;
    }

    void OnValidate()
    {
        for (int i = 0; i < 6; i++) 
            slots.Add(new List<ItemSlot>());

        for (int i = 0; i < slotParent.childCount; i++) 
        {
            var slot = slotParent.GetChild(i).GetComponent<ItemSlot>();
            int category = slot.category;
            slots[category].Add(slot);
        }
    }

    void Awake()
    {
        foreach(var item in itemSO.items)
            item.itemCount = 0;

        AddItemByItemCode("ITEM_TIER_2_PLASMA");
        AddItemByItemCode("ITEM_TIER_1_CARBON");
        AddItemByItemCode("ITEM_TIER_1_STEEL");
    }

    /// <summary>
    /// slot�� ������� ���� �� ȣ���. �κ��丮 ���� ���Կ� ������ �߰�
    /// </summary>
    public void UpdateSlot()
    {
        InitSlots();

        for (int i = 0; i < items.Count; i++)
        {
            int category = items[i].data.Category;
            var currentSlot = slots[category][counts[category]];
            currentSlot.gameObject.SetActive(true);
            currentSlot.item = items[i];
            currentSlot.GetComponentInChildren<TextMeshProUGUI>().text = items[i].itemCount.ToString();
            counts[category]++;
        }
    }

    /// <summary>
    /// slot �ʱ�ȭ
    /// </summary>
    void InitSlots()
    {
        for (int i = 0; i < slots.Count; i++)
        {
            for (int j = 0; j < slots[i].Count; j++)
                slots[i][j].gameObject.SetActive(false);

            counts[i] = 0;
        }
    }





    /// <summary>
    /// �κ��丮�� ItemBase�� �̿��Ͽ� ������ �߰�
    /// </summary>
    /// <param name="_item"></param>
    public void AddItem(ItemBase _item)
    {
        foreach (var item in items)
        {
            if (item == _item)
            {
                item.itemCount++;
                UpdateSlot();
                return;
            }
        }
        _item.itemCount++;
        items.Add(_item);
        UpdateSlot();
    }

    /// <summary>
    /// �κ��丮�� itemCode�� �̿��Ͽ� ������ �߰�
    /// </summary>
    /// <param name="itemCode"></param>
    public void AddItemByItemCode(string _itemCode)
    {
        for (int i = 0; i < itemSO.items.Length; i++)
            if (itemSO.items[i].itemCode == _itemCode)
                AddItem(itemSO.items[i]);
    }





    /// <summary>
    /// �κ��丮���� ������ ����
    /// </summary>
    /// <param name="_item"></param>
    public void RemoveItem(ItemBase _item)
    {
        _item.itemCount--;

        if (_item.itemCount == 0)
        {
            for (int i = 0; i < items.Count; i++)
            {
                if (items[i] == _item)
                {
                    items.RemoveAt(i);
                    break;
                }
            }
        }
        
        UpdateSlot();
    }

    public void RemoveItemByCode(string _itemCode)
    {
        ItemBase item;
        for (int i = 0; i < itemSO.items.Length; i++)
            if (itemSO.items[i].itemCode == _itemCode)
            {
                item = itemSO.items[i];
                RemoveItem(item);
            }
    }



    /// <summary>
    /// �κ��丮 ���� Ư�� ������ �����ϴ��� üũ
    /// </summary>
    /// <param name="itemCode"></param>
    /// <returns></returns>
    public bool CheckInventoryItem(string _itemCode)
    {
        foreach(var item in items)
        {
            if (item.itemCode == _itemCode)
                return true;
        }
        return false;
    }
}
