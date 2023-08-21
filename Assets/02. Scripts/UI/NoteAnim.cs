using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class NoteAnim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("Buttons")]
    [SerializeField] Button closeBtn;
    [SerializeField] Button openBtn;
    [SerializeField] Button nextDayBtn;

    [Header("Box Objects")]
    [SerializeField] GameObject boxTop;
    [SerializeField] GameObject boxBottom;
    [SerializeField] Image[] notePanels;

    [Header("Note Objects")]
    [SerializeField] Image blackPanel;
    [SerializeField] Text dayText;
    [SerializeField] GameObject nextPage;
    [SerializeField] GameObject prevPage;
    [SerializeField] GameObject nextDay;
    [SerializeField] GameObject inventoryUI;
    [SerializeField] GameObject noteBackground_Back;

    Vector2 originalPos;

    bool isOpen = false;
    int dayCount = 1;

    [SerializeField] NoteController noteController;

    void Start()
    {
        originalPos = transform.position;
        Initialize();
    }

    void Initialize()
    {
        foreach (var notePanel in notePanels)
            notePanel.DOFade(0f, 0f);

        blackPanel.DOFade(0f, 0f);
        closeBtn.gameObject.SetActive(false);
        nextPage.SetActive(false);
        prevPage.SetActive(false);
        nextDay.SetActive(false);
        blackPanel.gameObject.SetActive(false);
        dayText.gameObject.SetActive(false);
        noteBackground_Back.SetActive(false);

        openBtn.onClick.AddListener(Open_Anim);
        closeBtn.onClick.AddListener(Close_Anim);
        nextDayBtn.onClick.AddListener(NextDayEvent);
    }

    /// <summary>
    /// ���� ���콺 ȣ��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOpen)
            transform.DOMoveY(originalPos.y + 80f, 0.5f);
    }

    /// <summary>
    /// ���� ���콺 ȣ�� ����
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        transform.DOMoveY(originalPos.y, 0.5f);
    }


    /// <summary>
    /// ���� ���� �ִϸ��̼�
    /// </summary>
    public void Open_Anim()
    {
        if (!isOpen)
        {
            Debug.LogError("Open_Anim");

            DOTween.Kill(gameObject);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(transform.DOMoveY(originalPos.y, 0.5f))
                .Join(boxTop.transform.DOMoveY(960f, 0.5f))
                .Join(boxBottom.transform.DOMoveY(115f, 0.5f))
                .AppendCallback(() =>
                {
                    noteBackground_Back.SetActive(true);
                    foreach (var notePanel in notePanels)
                        notePanel.DOFade(1f, 0.5f);
                })
                .OnComplete(() => OpenBox());

            
            closeBtn.DOKill();
            openBtn.DOKill();

            isOpen = true;
            openBtn.gameObject.SetActive(false);
            closeBtn.gameObject.SetActive(false);
            sequence.Play();
        }
    }
    /// <summary>
    /// ���� ���� �ִϸ��̼�
    /// </summary>
    public void Close_Anim()
    {
        if (isOpen)
        {
            noteController.CloseBox();
            nextPage.SetActive(false);
            prevPage.SetActive(false);
            nextDay.SetActive(false);
            dayText.gameObject.SetActive(false);

            DOTween.Kill(gameObject);

            Sequence sequence = DOTween.Sequence();
            sequence.AppendCallback(() =>
            {
                foreach (var notePanel in notePanels)
                    notePanel.DOFade(0f, 0.5f);
            })
                .AppendInterval(0.5f)
                .Append(boxTop.transform.DOMoveY(10f, 0.5f))
                .Join(boxBottom.transform.DOMoveY(10f, 0.5f))
                .OnComplete(() => CloseBox());

            
            closeBtn.DOKill();
            openBtn.DOKill();

            openBtn.gameObject.SetActive(false);
            closeBtn.gameObject.SetActive(false);
            inventoryUI.gameObject.SetActive(false);
            sequence.Play();
        }
    }


    /// <summary>
    /// ���� ���� �Ѿ�� �ִϸ��̼�(���̵�ƿ�)
    /// </summary>
    void NextDayEvent()
    {
        Close_Anim();
        blackPanel.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(blackPanel.DOFade(1f, 0.5f)).SetEase(Ease.InQuint)
            .AppendInterval(0.5f)
            .Append(blackPanel.DOFade(0f, 0.5f + 0.5f))
            .OnComplete(() => NewDay());
        sequence.Play();
    }


    /// <summary>
    /// ���� ���� �ݹ��Լ�
    /// </summary>
    void OpenBox()
    {
        openBtn.gameObject.SetActive(false);
        closeBtn.gameObject.SetActive(true);
        nextPage.SetActive(true);
        prevPage.SetActive(true);
        dayText.gameObject.SetActive(true);

        UIManager.instance.AddCurrUIName(StringUtility.UI_NOTE);
        noteController.OpenBox();
    }
    /// <summary>
    /// ���� ���� �ݹ��Լ�
    /// </summary>
    void CloseBox()
    {
        isOpen = false;
        openBtn.gameObject.SetActive(true);
        closeBtn.gameObject.SetActive(false);
        noteBackground_Back.SetActive(false);

        UIManager.instance.PopCurrUI();
    }
    /// <summary>
    /// ���� ��ư �ݹ��Լ�
    /// </summary>
    void NewDay()
    {
        blackPanel.gameObject.SetActive(false);
        dayText.text = "Day " + ++dayCount;
        noteController.newDay = true;
        MapController.instance.AllowMouseEvent();
    }

    /// <summary>
    /// ��Ʈ�� �����ִ� �� üũ
    /// </summary>
    /// <returns></returns>
    public bool GetIsOpen()
    {
        return isOpen;
    }
}