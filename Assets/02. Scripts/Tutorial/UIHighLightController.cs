using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIHighLightController : MonoBehaviour
{
    [SerializeField] HighLight[] hightLights;
    [SerializeField] GameObject highLightImg;
    [SerializeField] public Dictionary<string, HighLight> dic_highLights = new Dictionary<string, HighLight>();

    private void Start()
    {
        foreach(var h in hightLights)
        {
            dic_highLights.Add(h.objectID, h);
        }
    }

    public void ShowHighLight(string _objectID, string _waitUntilStatusName)
    {
        if(dic_highLights.TryGetValue(_objectID, out HighLight h))
        {
            highLightImg.GetComponent<RectTransform>().sizeDelta = h.area.sizeDelta;

            StartCoroutine(WaitForPositionUpdate(h));

            StartCoroutine(HideHighLightWhenAction(h, _waitUntilStatusName));
        }
        else
        {
            Debug.LogError($"invalid highlight object name : {_objectID}");
        }
    }

    IEnumerator WaitForPositionUpdate(HighLight h)
    {
        yield return new WaitForEndOfFrame();

        Vector2 canvasPosition = h.area.position;

        highLightImg.GetComponent<RectTransform>().position = canvasPosition;

        highLightImg.SetActive(true);
        h.Show();
    }

    private IEnumerator HideHighLightWhenAction(HighLight _h, string _waitUntilStatusName)
    {
        yield return new WaitUntil(() => UIManager.instance.isUIStatus(_waitUntilStatusName));

        _h.Hide();
        highLightImg.SetActive(false);
    }

    public void ShowBtnHighLight(string _objectID)
    {
        if (dic_highLights.TryGetValue(_objectID, out HighLight h))
        {
            highLightImg.GetComponent<RectTransform>().sizeDelta = h.area.sizeDelta;

            StartCoroutine(WaitForPositionUpdate(h));

            if (_objectID == "ClickCraftItems")
                StartCoroutine(HideClickCraftItems(h));
            else if (_objectID == "ClickResultItem")
                StartCoroutine(HideClickResultItem(h));
        }
        else
        {
            Debug.LogError($"invalid highlight object name : {_objectID}");
        }
    }

    private IEnumerator HideClickCraftItems(HighLight _h)
    {
        yield return new WaitUntil(() => UIManager.instance.GetCraftingUiController().isMoreThanThree());

        _h.Hide();
        highLightImg.SetActive(false);
        ShowBtnHighLight("ClickResultItem");
    }

    private IEnumerator HideClickResultItem(HighLight _h)
    {
        yield return new WaitUntil(() => UIManager.instance.GetInventoryController().CheckInventoryItem("ITEM_BATTERY"));

        _h.Hide();
        highLightImg.SetActive(false);
    }
}
