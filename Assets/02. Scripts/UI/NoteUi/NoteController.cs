using UnityEngine;
using UnityEngine.UI;

public class NoteController : ControllerBase
{
    [Header("Note Objects")]
    [SerializeField] Text dayText;
    [SerializeField] GameObject noteBackground;

    [Header("Buttons")]
    [SerializeField] Button nextPageBtn;
    [SerializeField] Button prevPageBtn;
    [SerializeField] Button closeBtn;

    NotePageBase[] notePages;

    bool isNewDay = true;
    bool isOpen = false;
    int dayCount = 0;
    int pageNum = 0;





    public override EControllerType GetControllerType()
    {
        return EControllerType.NOTE;
    }





    void Start()
    {
        Init();
    }

    void Init()
    {
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
        notePages = UIManager.instance.GetNextDayController().GetNotePageArray();
    }





    /// <summary>
    /// ��Ʈ �� �� ȣ��
    /// </summary>
    public void OpenNote()
    {
        if (notePages.Length == 0) return;
        if (isOpen == false)
        {
            isOpen = true;
            ActiveObjects(true);

            ActiveAndPlayPage();

            if (isNewDay == true)
                isNewDay = false;

            ChangePageButton();

            App.instance.GetSoundManager().PlaySFX("SFX_Note_Open");
            UIManager.instance.AddCurrUIName(StringUtility.UI_NOTE);
        }
    }

    /// <summary>
    /// ��Ʈ ���� �� ȣ��
    /// </summary>
    public void CloseNote()
    {
        if (isOpen == true)
        {
            isOpen = false;
            ActiveObjects(false);
            ActiveNextBtnAndPrevBtn(false, false);

            notePages[pageNum].gameObject.SetActive(false);

            App.instance.GetSoundManager().PlaySFX("SFX_Note_Close");
            UIManager.instance.PopCurrUI();
        }
    }

    /// <summary>
    /// ��Ʈ ���� ���� �� ��ġ�� ������Ʈ Ȱ��ȭ/��Ȱ��ȭ
    /// </summary>
    /// <param name="isEnable"></param>
    void ActiveObjects(bool _isEnable)
    {
        closeBtn.gameObject.SetActive(_isEnable);
        dayText.gameObject.SetActive(_isEnable);
        noteBackground.SetActive(_isEnable);
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
    public void GoToNextPage()
    {
        if (pageNum + 1 > notePages.Length - 1)
            return;

        ChangePage(pageNum + 1);
    }

    /// <summary>
    /// ���� ������ ��ư Ŭ�� �� ȣ��
    /// </summary>
    public void GoToPrevPage()
    {
        if (pageNum - 1 < 0)
            return;

        ChangePage(pageNum - 1);
    }

    /// <summary>
    /// ����/���� �������� �̵�
    /// </summary>
    /// <param name="index"></param>
    void ChangePage(int _index)
    {
        notePages[pageNum].gameObject.SetActive(false);
        pageNum = _index;
        ActiveAndPlayPage();
        ChangePageButton();
    }

    /// <summary>
    /// ���ο� ������ Ȱ��ȭ �� ������ ����(Yarn ����)
    /// </summary>
    void ActiveAndPlayPage()
    {
        notePages[pageNum].gameObject.SetActive(true);
        notePages[pageNum].PlayPageAction();
    }

    public void ChangePageForce(int _index) //�ÿ�ȸ������ �ӽ÷� ������� �Լ�.. �����ʹ�
    {
        pageNum = _index;

        Debug.Log("����");
        if (isOpen == false)
        {
            OpenNote();
            Debug.Log("����");
        }


        for (int i = 0; i < notePages.Length; i++)
        {
            notePages[i].gameObject.SetActive(false);
            Debug.Log(i + "����");
        }

        notePages[_index].gameObject.SetActive(true);
        Debug.Log(_index);
        notePages[_index].PlayPageAction();
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
    void ActiveNextBtnAndPrevBtn(bool _nextBtnEnable, bool _prevBtnEnable)
    {
        nextPageBtn.gameObject.SetActive(_nextBtnEnable);
        prevPageBtn.gameObject.SetActive(_prevBtnEnable);
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

    public void SetPageNum(ENotePageType _type)
    {
        for (int i = 0; i < notePages.Length; i++)
        {
            if (notePages[i].GetENotePageType() == _type)
                pageNum = i;
        }
    }
    #endregion
}
