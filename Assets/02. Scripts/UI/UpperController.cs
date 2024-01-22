using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class UpperController : MonoBehaviour
{
    [SerializeField] ItemSO itemSO;

    [Header("ItemCountText")]
    [SerializeField] TextMeshProUGUI steelText;
    [SerializeField] TextMeshProUGUI carbonText;
    [SerializeField] TextMeshProUGUI plasmaText;
    [SerializeField] TextMeshProUGUI bulletText;
    [SerializeField] TextMeshProUGUI durabilityText;
    [SerializeField] TextMeshProUGUI powderText;
    [SerializeField] TextMeshProUGUI gasText;
    [SerializeField] TextMeshProUGUI rubberText;

    ItemBase steel;
    ItemBase carbon;
    ItemBase plasma;
    ItemBase powder;
    ItemBase gas;
    ItemBase rubber;
    ItemBase bullet;

    void Start()
    {
        foreach (var item in itemSO.items)
        {
            if (item.data.Code == "ITEM_STEEL") steel = item;
            else if (item.data.Code == "ITEM_CARBON") carbon = item;
            else if (item.data.Code == "ITEM_PLASMA") plasma = item;
            else if (item.data.Code == "ITEM_POWDER") powder = item;
            else if (item.data.Code == "ITEM_GAS") gas = item;
            else if (item.data.Code == "ITEM_RUBBER") rubber = item;
            else if (item.data.Code == "ITEM_BULLET") bullet = item;
        }

        StartCoroutine(InitData());
    }

    IEnumerator InitData()
    {
        yield return new WaitUntil(() => App.instance.GetMapManager().mapController);
        yield return new WaitUntil(() => App.instance.GetMapManager().mapController.Player != null);

        UpdateItemCount();
        UpdateAfterFight();
    }

    public void UpdateItemCount()
    {
        steelText.text = steel.itemCount.ToString("D3");
        carbonText.text = carbon.itemCount.ToString("D3");
        plasmaText.text = plasma.itemCount.ToString("D3");
        powderText.text = powder.itemCount.ToString("D3");
        gasText.text = gas.itemCount.ToString("D3");
        rubberText.text = rubber.itemCount.ToString("D3");
        bulletText.text = bullet.itemCount.ToString("D3");
    }

    public void UpdateDurabillity()
    {
        durabilityText.text = App.instance.GetMapManager().mapController.Player.Durability.ToString("D3");
    }

    public void UpdateAfterFight()
    {
        bulletText.text = bullet.itemCount.ToString("D3");
        durabilityText.text = App.instance.GetMapManager().mapController.Player.Durability.ToString("D3");
    }
}
