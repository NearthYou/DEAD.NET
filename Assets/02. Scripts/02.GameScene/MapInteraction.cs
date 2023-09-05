using UnityEngine;
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
        App.instance.GetMapManager().SetMapCameraPriority(true);
    }
}