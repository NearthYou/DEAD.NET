using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class WorkBenchInteraction : MonoBehaviour, IPointerClickHandler
{
    public UnityEvent onClickEvent;
    [SerializeField] Transform cube;
    [SerializeField] Transform cubeElse;

    float cubeInitPositionY;
    float cubeElseInitPositionY;

    void Start()
    {
        cubeInitPositionY = cube.position.y;
        cubeElseInitPositionY = cubeElse.position.y;
    }

    /// <summary>
    /// �۾��� Ŭ�� �� �̺�Ʈ �߻� �Լ�
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        onClickEvent?.Invoke();
    }

    public void StartAnim()
    {
        cube.DOMoveY(cubeInitPositionY + 1f, 5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        cubeElse.DOMoveY(cubeElseInitPositionY + 2f, 5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }
}
