using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;

public class NoteInteraction : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClickEvent;

    /// <summary>
    /// ��Ʈ Ŭ�� �� �̺�Ʈ �߻� �Լ�
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickEvent?.Invoke();
    }
}
