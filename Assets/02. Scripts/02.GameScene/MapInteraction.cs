using Cinemachine;
using DG.Tweening;
using UnityEngine;
using System.Collections;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class MapInteraction : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClickEvent;

    /// <summary>
    /// ���� Ŭ�� �� �̺�Ʈ �߻� �Լ�
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        if (UIManager.instance.isUIStatus("UI_NORMAL") == false) return;
        UIManager.instance.GetNextDayController().GoToMap();
    }
}