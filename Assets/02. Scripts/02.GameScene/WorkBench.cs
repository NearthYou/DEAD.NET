using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class WorkBench : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClickEvent;
   
    /// <summary>
    /// �۾��� Ŭ�� �� �̺�Ʈ �߻� �Լ�
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        UIManager.instance.AddCurrUIName(StringUtility.UI_CRAFTING);
        onClickEvent.Invoke();
    }
}
