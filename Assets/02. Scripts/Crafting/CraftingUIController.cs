using System.Collections;
using System.Collections.Generic;
using TMPro.EditorUtilities;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CraftingUIController : MonoBehaviour
{
    [SerializeField] Transform slotParent;
    //[SerializeField] Sprite[] craftTypeImage;
    [SerializeField] GameObject inventoryUi;

    private ItemSlot[] slots;
    public List<Transform> slotTransforms;

    InventoryPage inventoryPage;

    public List<ItemBase> items;
    //private List<Image> craftTypeImages;
    public List<ItemCombineData> itemCombines;
    string[] combinationCodes = new string[9];

    void Awake()
    {
        inventoryPage = GameObject.Find("Inventory").GetComponent<InventoryPage>();

        slots = slotParent.GetComponentsInChildren<ItemSlot>();
        for (int i = 0; i < slotParent.childCount; i++)
        {
            if (slotParent.GetChild(i))
                slotTransforms.Add(slotParent.GetChild(i));
            //craftTypeImages.Add(slotTransforms[i].GetChild(1).GetComponent<Image>());
        }
    }

    void Start()
    {
        for(int i = 1001; i < 2000; i++)
        {
            DataManager.instance.itemCombineData.TryGetValue(i, out ItemCombineData itemData);

            if (itemData != null)
                itemCombines.Add(itemData);
            else
                break;
        }
       
        gameObject.SetActive(false);

        for (int i = 0; i < slots.Length; i++)
        {
            slotTransforms[i].gameObject.SetActive(false);
            slots[i].item = null;
            //craftTypeImages[i].GetComponent<Image>().sprite = craftTypeImage[0];
        }
    }

    /// <summary>
    /// CraftBag ���ΰ�ħ(?)
    /// </summary>
    public void FreshCraftingBag()
    {
        for (int i = 0; i < items.Count; i++)
        {
            slotTransforms[i].gameObject.SetActive(false);
            slots[i].item = null;
            //craftTypeImages[i].GetComponent<Image>().sprite = craftTypeImage[0];
        }

        int j = 0;

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
        inventoryPage.RemoveItem(_item);
    }

    /// <summary>
    /// Exit ��ư ������ �� �κ��丮�� ������ ��ȯ
    /// </summary>
    public void ReturnItem()
    {
        gameObject.SetActive(false);
        inventoryUi.SetActive(false);

        for (int i = 0; i < items.Count; i++)
        {
            inventoryPage.AddItem(items[i]);
            slotTransforms[i].gameObject.SetActive(false);
            slots[i].item = null;
            //craftTypeImages[i].GetComponent<Image>().sprite = craftTypeImage[0];
        }
    }

    /// <summary>
    /// ����ǥ ��
    /// </summary>
    public void CombineItem()
    {
        int flag = 0; // 0: ��ġ, 1: ����ġ
        for (int i = 0; i < itemCombines.Count; i++)
        {
            flag = 0;

            combinationCodes[0] = itemCombines[i].Material_1;
            combinationCodes[1] = itemCombines[i].Material_2;
            combinationCodes[2] = itemCombines[i].Material_3;
            combinationCodes[3] = itemCombines[i].Material_4;
            combinationCodes[4] = itemCombines[i].Material_5;
            combinationCodes[5] = itemCombines[i].Material_6;
            combinationCodes[6] = itemCombines[i].Material_7;
            combinationCodes[7] = itemCombines[i].Material_8;
            combinationCodes[8] = itemCombines[i].Result;

            for (int j = 0; j < items.Count; j++)
            {
                if (flag == 1) break;

                for (int k = 0; k < 8; k++)
                {
                    if (combinationCodes[k] == "1") continue;
                    if (combinationCodes[k] != items[j].itemCode)
                    {
                        flag = 1;
                        break;
                    }
                    else
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
                AddCombineResult(combinationCodes[8]);
                break;
            }
        }
    }

    /// <summary>
    /// ���� ��� ������ CraftBag�� ǥ��
    /// </summary>
    public void AddCombineResult(string resultItemCode)
    {

    }
}
