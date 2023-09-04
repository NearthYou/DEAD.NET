using DG.Tweening;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SetNextDay : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [SerializeField] Image blackPanel;
    NotePage[] pages;

    [Header("Gauage Obejcts")]
    [SerializeField] Image gaugeImage;
    [SerializeField] float fillSpeed = 1.0f;
    [SerializeField] float maxGaugeValue = 100.0f;
    float currentGaugeValue = 0.0f;
    bool isFilling = false;

    [Header("Quest Objects")]
    [SerializeField] GameObject questPrefab;
    [SerializeField] Transform questParent;

    [Header("Alarm Objects")]
    [SerializeField] GameObject newAlarm;
    [SerializeField] GameObject resultAlarm;
    [SerializeField] GameObject cautionAlarm;





    void Awake()
    {
        pages = GameObject.Find("Page_Back").GetComponentsInChildren<NotePage>(includeInactive: true);

        Init();
    }

    void Update()
    {
        if (isFilling) //��ư ������ ����
        {
            FillGauge();
            if (currentGaugeValue >= maxGaugeValue) //�������� �� ���� ���� ���� �̵�
            {
                isFilling = false;
                NextDayEvent();
            }
        }
    }





    /// <summary>
    /// �°� �ʱ�ȭ �Լ� ����
    /// </summary>
    void Init()
    {
        InitBlackPanel();
        InitGauageUI();
        InitPageEnabled();
        InitQuestList();
        InitAlarm();
    }

    #region Inits
    /// <summary>
    /// Ȱ��ȭ �ƴ� BlackPanel �ٽ� ���b��ȭ (BlackPanel�� ���� ���� �Ѿ �� ��� ���̴� � ȭ��)
    /// </summary>
    void InitBlackPanel()
    {
        Sequence sequence = DOTween.Sequence();
        sequence.Append(blackPanel.DOFade(0f, 1f))
            .OnComplete(() => blackPanel.gameObject.SetActive(false));
        sequence.Play();   
    }

    /// <summary>
    /// ��ư ������ �ʱ�ȭ
    /// </summary>
    void InitGauageUI()
    {
        currentGaugeValue = 0.0f;
        gaugeImage.fillAmount = 0;
    }

    /// <summary>
    /// ��Ʈ ������ �ʱ�ȭ (dialogueRunner ���߱�, ������ ��Ȱ��ȭ) --> �߰� �۾� �ʿ�. ������ �����ؾ���.
    /// </summary>
    void InitPageEnabled()
    {
        foreach (NotePage page in pages)
        {
            page.StopDialogue();
            page.gameObject.SetActive(false);
            //���� ���ο� ���� ���۵� ������ ������ �޾ƿ� page.SetPageEnabled() ȣ���Ͽ� �� �Ѱ��ֱ�
        }
    }

    /// <summary>
    /// ����Ʈ ��� �ʱ�ȭ (�����Ǿ��� ������ ��� �ı�)
    /// </summary>
    void InitQuestList()
    {
        Quest[] quests = questParent.GetComponentsInChildren<Quest>();
        foreach (Quest quest in quests)
        {
            Destroy(quest.gameObject);
        }
    }

    /// <summary>
    /// �˸� ��� �ʱ�ȭ --> ����� �ʿ��� �˸��� SetActive(true)�ؼ� ��� ���ε�, �� ���� ����� ��� ��. / ���������� �߰� �۾� �ʿ�. ������ �����ؾ���.
    /// </summary>
    void InitAlarm()
    {
        newAlarm.SetActive(false);
        resultAlarm.SetActive(false);
        cautionAlarm.SetActive(false);
    }
    #endregion





    /// <summary>
    /// ���� ���� �� �� BlackPanel Ȱ��ȭ/���̵���
    /// </summary>
    void NextDayEvent()
    {
        blackPanel.gameObject.SetActive(true);
        Sequence sequence = DOTween.Sequence();
        sequence.Append(blackPanel.DOFade(1f, 0.5f)).SetEase(Ease.InQuint)
            .AppendInterval(0.5f)
            .OnComplete(() => NextDayEventCallBack());
        sequence.Play();
    }

    /// <summary>
    /// �ٷ� ���� NextDayEvent()�� �ݹ��Լ� (�ʱ�ȭ �۾�)
    /// </summary>
    void NextDayEventCallBack()
    {
        Init();
        GameManager.instance.SetPrioryty(false);

        UIManager.instance.GetNoteController().SetNextDay();
        App.instance.GetMapManager().AllowMouseEvent(true);
        MapController.instance.NextDay();
    }





    #region Gauage
    /// <summary>
    /// ��ư�� ������ ���� ��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        isFilling = true;
    }

    /// <summary>
    /// ��ư���� ���� ��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        isFilling = false;
        InitGauageUI();
    }

    /// <summary>
    /// ������ ä��
    /// </summary>
    void FillGauge()
    {
        currentGaugeValue += fillSpeed * Time.deltaTime;
        UpdateGaugeUI();
    }

    /// <summary>
    /// �������� ���� Ui ����
    /// </summary>
    void UpdateGaugeUI()
    {
        gaugeImage.fillAmount = currentGaugeValue / maxGaugeValue;
    }
    #endregion





    #region PageSetting
    /// <summary>
    /// ���ο� ���� ���̴� �������� ��� NoteController�� �迭�� ����. (page.GetPageEnableToday()�Լ��� ��� ���� Ȯ��)
    /// </summary>
    /// <returns></returns>
    public NotePage[] GetNotePageArray()
    {
        List<NotePage> todayPages = new List<NotePage>();
        foreach (NotePage page in pages)
        {
            if (page.GetPageEnableToday())
            {
                todayPages.Add(page);
            }
        }

        return todayPages.ToArray(); ;
    }
    #endregion





    #region QuestSetting
    /// <summary>
    /// ����Ʈ ������ �߰� --> �߰� �۾� �ʿ�.. �׸��� �� �� �����ϰ� �Լ� ������ ����.
    /// </summary>
    /// <param name="type"></param>
    /// <param name="text"></param>
    void AddQuest(EQuestType type, string text)
    {
        GameObject obj = Instantiate(questPrefab, questParent);
        Quest quest = obj.GetComponent<Quest>();
        quest.SetEQuestType(type);
        quest.SetText(text);
        quest.SetQuestTypeText();
        quest.SetQuestTypeImage();
        SetQuestList();
    }

    /// <summary>
    /// ����Ʈ ����Ʈ ����(?) ��������Ʈ�� ����, ��������Ʈ�� �Ʒ��� �߰�. --> ���� AddQuest�Լ� ���� �Ǹ� ���������� �굵 �պ��� ����. �� ���� ����� ������!
    /// </summary>
    void SetQuestList()
    {
        Quest[] quests = questParent.GetComponentsInChildren<Quest>();
        foreach (Quest quest in quests)
        {
            if(quest.GetEQuestType() == EQuestType.Main)
                quest.transform.SetAsFirstSibling();
            else
                quest.transform.SetAsLastSibling();
        }
    }
    #endregion





    #region ForTest
    public void AddMainQuestBtn() //�׽�Ʈ�� �ӽ� �Լ�. ��������Ʈ �߰� ��ư
    {
        AddQuest(EQuestType.Main, "�����Դϴ�");
    }

    public void AddSubQuestBtn() //�׽�Ʈ�� �ӽ� �Լ�. ��������Ʈ �߰� ��ư
    {
        AddQuest(EQuestType.Sub, "���ÿ�");
    }
    public void AddResultPage() //�׽�Ʈ�� �ӽ� �Լ�. ���� ���� ��� ������ Ȱ��ȭ ��ư
    {
        pages[0].SetPageEnabled(true);
    }
    public void RemoveResultPage() //�׽�Ʈ�� �ӽ� �Լ�. ���� ���� ��� ������ ��Ȱ��ȭ ��ư
    {
        pages[0].SetPageEnabled(false);
    }
    public void AddSelectPage() //�׽�Ʈ�� �ӽ� �Լ�. ���� ���� ���� ������ Ȱ��ȭ ��ư
    {
        pages[1].SetPageEnabled(true);
    }
    public void RemoveSelectPage() //�׽�Ʈ�� �ӽ� �Լ�. ���� ���� ���� ������ ��Ȱ��ȭ ��ư
    {
        pages[1].SetPageEnabled(false);
    }
    #endregion





    //�ƹ�ư ��ü������ ���ο� �� �� �� �� �� ������ Ȯ���ؾ� �ϴ� �͵��� ���� �߰� �۾� �ʿ���(����Ʈ, ��Ʈ ������, �˸�)
}