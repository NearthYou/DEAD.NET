using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using UnityEngine.EventSystems;

public class NoteManager : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    public Button closeBtn;
    public Button openBtn;
    public Button nextDayBtn;

    public GameObject boxTop;
    public GameObject boxBottom;
    public GameObject notePanel;
    public Image blackPanel;
    public Text dayText;

    public GameObject nextPage;
    public GameObject prevPage;
    public GameObject nextDay;

    public Sprite[] btnImages;

    private Vector2 originalPos;
    private Vector2 topOriginalPos;
    private Vector2 bottomOriginalPos;

    public bool isOpen = false;
    private int dayCount = 1;

    TempScript pageManager;

    void Start()
    {
        pageManager = GameObject.Find("PageManager").GetComponent<TempScript>();

        notePanel.GetComponent<Image>().DOFade(0f, 0f);
        blackPanel.DOFade(0f, 0f);

        topOriginalPos = boxTop.transform.position;
        bottomOriginalPos = boxBottom.transform.position;
        originalPos = transform.position;

        openBtn.onClick.AddListener(Open_Anim);
        closeBtn.onClick.AddListener(Close_Anim);
        nextDayBtn.onClick.AddListener(NextDayEvent);

        nextPage.SetActive(false);
        prevPage.SetActive(false);
        nextDay.SetActive(false);
        blackPanel.gameObject.SetActive(false);
        dayText.gameObject.SetActive(false);
    }

    /// <summary>
    /// ���� ���콺 ȣ��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isOpen)
            transform.DOMoveY(originalPos.y + 100f, 0.5f);
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
    void Open_Anim()
    {
        if (!isOpen)
        {
            Sequence sequence = DOTween.Sequence();
            sequence.Append(boxTop.transform.DOMoveY(topOriginalPos.y + 900f, 0.5f))
                .Join(boxBottom.transform.DOMoveY(bottomOriginalPos.y + 150f, 0.5f))
                .Join(transform.DOMoveY(originalPos.y, 0.5f))
                .Append(notePanel.GetComponent<Image>().DOFade(1f, 0.5f))
                .OnComplete(() => OpenBox());

            DOTween.Kill(gameObject);
            closeBtn.DOKill();
            openBtn.DOKill();

            isOpen = true;
            openBtn.image.sprite = btnImages[0];
            closeBtn.image.sprite = btnImages[0];
            sequence.Play();
        }
    }
    /// <summary>
    /// ���� ���� �ִϸ��̼�
    /// </summary>
    void Close_Anim()
    {
        if (isOpen)
        {
            pageManager.CloseBox();
            nextPage.SetActive(false);
            prevPage.SetActive(false);
            nextDay.SetActive(false);

            Sequence sequence = DOTween.Sequence();
            sequence.Append(notePanel.GetComponent<Image>().DOFade(0f, 0.5f))
                .Append(boxTop.transform.DOMoveY(topOriginalPos.y, 0.5f))
                .Join(boxBottom.transform.DOMoveY(bottomOriginalPos.y, 0.5f))
                .OnComplete(() => CloseBox());

            DOTween.Kill(gameObject);
            closeBtn.DOKill();
            openBtn.DOKill();

            isOpen = false;
            openBtn.image.sprite = btnImages[0];
            closeBtn.image.sprite = btnImages[0];
            dayText.gameObject.SetActive(false);
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
        sequence.Append(blackPanel.DOFade(1f, 1f)).SetEase(Ease.InQuint)
            .AppendInterval(0.5f)
            .Append(blackPanel.DOFade(0f, 1f))
            .OnComplete(() => NewDay());
        sequence.Play();
    }

    /// <summary>
    /// ���� ���� �ݹ��Լ�
    /// </summary>
    void OpenBox()
    {
        openBtn.image.sprite = btnImages[0];
        closeBtn.image.sprite = btnImages[1];
        nextPage.SetActive(true);
        prevPage.SetActive(true);
        dayText.gameObject.SetActive(true);
        pageManager.OpenBox();
    }
    /// <summary>
    /// ���� ���� �ݹ��Լ�
    /// </summary>
    void CloseBox()
    {
        openBtn.image.sprite = btnImages[1];
        closeBtn.image.sprite = btnImages[0];
    }
    /// <summary>
    /// ���� ��ư �ݹ��Լ�
    /// </summary>
    void NewDay()
    {
        blackPanel.gameObject.SetActive(false);
        dayText.text = "Day" + ++dayCount;
    }
}