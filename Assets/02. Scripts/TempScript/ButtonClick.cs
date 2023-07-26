using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ButtonClick : MonoBehaviour
{
    [SerializeField] ItemBase steel;
    [SerializeField] ItemBase plastic;
    [SerializeField] ItemBase gear;
    [SerializeField] InventoryPage inventoryPage;

    public void buttonClick()
    {
        Debug.Log("��ư�� ���Ƚ��ϴ�.");
    }

    public void CreateSteel()
    {
        inventoryPage.AddItem(steel);
    }
    public void CreatePlastic()
    {
        inventoryPage.AddItem(plastic);
    }

    public void CreateGear()
    {
        inventoryPage.AddItem(gear);
    }
}
