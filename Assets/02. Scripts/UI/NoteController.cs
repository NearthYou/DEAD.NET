using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using DG.Tweening;
using Unity.VisualScripting;

public class NoteController : MonoBehaviour
{
    [Header("Note Objects")]
    [SerializeField] Text dayText;
    [SerializeField] GameObject noteBackground;

    [Header("Buttons")]
    [SerializeField] Button nextPageBtn;
    [SerializeField] Button prevPageBtn;
    [SerializeField] Button closeBtn;

    NotePage[] notePages;

    bool isNewDay = true;
    bool isOpen = false;
    int dayCount = 0;
    int pageNum = 0;

    SetNextDay setNextDay; //controller�� manager�� ����..���





    void Start()
    {
        Init();
    }

    void Init()
    {
        setNextDay = GameObject.Find("NextDay_Btn").GetComponent<SetNextDay>();

        ActiveNextBtnAndPrevBtn(false, false);
        ActiveObjects(false);
        InitVariables();
    }

    /// <summary>
    /// ���� �ʱ�ȭ (ù �� ���� ���ο� ���� ���۵� ������ ȣ���)
    /// </summary>
    void InitVariables()
    {
        dayText.text = "Day " + ++dayCount;
        isNewDay = true;
        pageNum = 0;
        notePages = setNextDay.GetNotePageArray();
    }





    /// <summary>
    /// ��Ʈ �� �� ȣ��
    /// </summary>
    public void OpenNote()
    {
        if (notePages.Length == 0) return;
        if (!isOpen)
        {
            isOpen = true;
            ActiveObjects(true);

            EnableAndPlayPage();

            if (isNewDay)
                isNewDay = false;

            ChangePageButton();

            UIManager.instance.AddCurrUIName(StringUtility.UI_NOTE);
        }
    }

    /// <summary>
    /// ��Ʈ ���� �� ȣ��
    /// </summary>
    public void CloseNote()
    {
        if (isOpen)
        {
            isOpen = false;
            ActiveObjects(false);

            notePages[pageNum].gameObject.SetActive(false);

            UIManager.instance.PopCurrUI();
        }
    }

    /// <summary>
    /// ��Ʈ ���� ���� �� ��ġ�� ������Ʈ Ȱ��ȭ/��Ȱ��ȭ
    /// </summary>
    /// <param name="isEnable"></param>
    void ActiveObjects(bool isEnable)
    {
        closeBtn.gameObject.SetActive(isEnable);
        dayText.gameObject.SetActive(isEnable);
        noteBackground.SetActive(isEnable);
    }





    /// <summary>
    /// ���� ���� �Ǿ��� �� ȣ���. ���� �ʱ�ȭ �� ��Ʈ ����
    /// </summary>
    public void SetNextDay()
    {
        InitVariables();
        CloseNote();
    }





    /// <summary>
    /// ���� ������ ��ư Ŭ�� �� ȣ��
    /// </summary>
    public void NextPageEvent()
    {
        if (pageNum + 1 > notePages.Length - 1)
            return;

        ChangePage(pageNum + 1);
    }

    /// <summary>
    /// ���� ������ ��ư Ŭ�� �� ȣ��
    /// </summary>
    public void PrevPageEvent()
    {
        if (pageNum - 1 < 0)
            return;

        ChangePage(pageNum - 1);
    }

    /// <summary>
    /// ����/���� �������� �̵�
    /// </summary>
    /// <param name="index"></param>
    void ChangePage(int index)
    {
        notePages[pageNum].gameObject.SetActive(false);
        pageNum = index;
        EnableAndPlayPage();
        ChangePageButton();
    }

    /// <summary>
    /// ���ο� ������ Ȱ��ȭ �� ������ ����(Yarn ����)
    /// </summary>
    void EnableAndPlayPage()
    {
        notePages[pageNum].gameObject.SetActive(true);
        notePages[pageNum].PlayPageAction();
    }

    public void ChangePageForce(int index) //�ÿ�ȸ������ �ӽ÷� ������� �Լ�.. �����ʹ�
    {
        pageNum = index;

        Debug.Log("����");
        if (!isOpen)
        {
            OpenNote();
            Debug.Log("����");
        }


        for (int i = 0; i < notePages.Length; i++)
        {
            notePages[i].gameObject.SetActive(false);
            Debug.Log(i + "����");
        }

        notePages[index].gameObject.SetActive(true);
        Debug.Log(index);
        notePages[index].PlayPageAction();
        ChangePageButton();
    }





    /// <summary>
    /// �������� ���� ������ ����/���� ��ư Ȱ��ȭ ���� Ȯ��
    /// </summary>
    void ChangePageButton()
    {
        if (notePages.Length == 1)
            ActiveNextBtnAndPrevBtn(false, false);
        else
        {
            if (pageNum == 0)
                ActiveNextBtnAndPrevBtn(true, false);
            else if (pageNum == notePages.Length - 1)
                ActiveNextBtnAndPrevBtn(false, true);
            else
                ActiveNextBtnAndPrevBtn(true, true);
        }
    }

    /// <summary>
    /// ������ ����/���� ��ư Ȱ��ȭ �Ǵ� ��Ȱ��ȭ
    /// </summary>
    /// <param name="nextBtnEnable"></param>
    /// <param name="prevBtnEnable"></param>
    void ActiveNextBtnAndPrevBtn(bool nextBtnEnable, bool prevBtnEnable)
    {
        nextPageBtn.gameObject.SetActive(nextBtnEnable);
        prevPageBtn.gameObject.SetActive(prevBtnEnable);
    }





    #region GetAndSet
    public bool GetIsOpen()
    {
        return isOpen;
    }

    public bool GetNewDay()
    {
        return isNewDay;
    }

    public int GetDayCount()
    {
        return dayCount;
    }

    public void SetPageNum(ENotePageType type)
    {
        for (int i = 0; i < notePages.Length; i++)
        {
            if (notePages[i].GetENotePageType() == type)
            {
                pageNum = i;
            }
        }
    }
    #endregion





    public void TempOpenBtn() //�ӽ÷� ui������ ��Ʈ�� ��ġ�� ���� ���� �Լ�
    {
        OpenNote();
    }
}
