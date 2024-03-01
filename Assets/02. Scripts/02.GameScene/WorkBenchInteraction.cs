using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using DG.Tweening;

public class WorkBenchInteraction : MonoBehaviour, IPointerClickHandler, IPointerEnterHandler, IPointerExitHandler
{
    public UnityEvent onClickEvent;
    [SerializeField] Transform cube;
    [SerializeField] Transform cubeElse;
    [SerializeField] Sprite[] images;
    Image image;

    float cubeInitPositionY;
    float cubeElseInitPositionY;

    void Start()
    {
        cubeInitPositionY = cube.position.y;
        cubeElseInitPositionY = cubeElse.position.y;

        image = gameObject.GetComponent<Image>();
        SetOutline(false);
    }

    /// <summary>
    /// �۾��� Ŭ�� �� �̺�Ʈ �߻� �Լ�
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerClick(PointerEventData eventData)
    {
        SetOutline(false);
        onClickEvent?.Invoke();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (UIManager.instance.isUIStatus("UI_NORMAL") == true)
            SetOutline(true);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (UIManager.instance.isUIStatus("UI_NORMAL") == true)
            SetOutline(false);
    }

    void SetOutline(bool _isEnabled)
    {
        if (_isEnabled == true)
            image.sprite = images[0];
        else
            image.sprite = images[1];
    }

    public void StartAnim()
    {
        cube.DOMoveY(cubeInitPositionY + 1f, 5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
        cubeElse.DOMoveY(cubeElseInitPositionY + 2f, 5f).SetEase(Ease.Linear).SetLoops(-1, LoopType.Yoyo);
    }
}
