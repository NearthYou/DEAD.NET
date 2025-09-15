using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class ExplorerButton : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    bool isMouseEnter = false;
    [SerializeField] string text;

    void Start()
    {
        gameObject.GetComponent<Button>().onClick.AddListener(Explorer);
    }

    void Update()
    {
        if (isMouseEnter == true)
            ShowItemInfo();
    }

    public virtual void ShowItemInfo()
    {
        Vector3 mousePos = Input.mousePosition;
        UIManager.instance.GetInfoController().ShowItemInfo(text, mousePos);
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        isMouseEnter = false;
        UIManager.instance.GetInfoController().HideInfo();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (isMouseEnter == false && UIManager.instance.isUIStatus("UI_MAP"))
        {
            isMouseEnter = true;
            UIManager.instance.GetInfoController().isNew = true;
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (isMouseEnter == true)
        {
            isMouseEnter = false;
            UIManager.instance.GetInfoController().HideInfo();
        }
    }

    public void Explorer()
    {
        if (UIManager.instance.GetInventoryController().CheckFindorUsage())
        {
            if (App.instance.GetMapManager().CheckCanInstallDrone())
            {
                MapController.Instance.PreparingExplorer(true);
            }
        }
    }
}