using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class NoteController : ControllerBase
{
    [Header("Note Objects")]
    [SerializeField] Text dayText;
    [SerializeField] GameObject noteBackground;

    [Header("Buttons")]
    [SerializeField] Button nextPageBtn;
    [SerializeField] Button prevPageBtn;
    [SerializeField] Button closeBtn;

    public NotePageBase[] pages;
    NotePageBase[] notePages;

    bool isNewDay = true;
    bool isOpen = false;
    int dayCount = 0;
    int pageNum = 0;





    public override EControllerType GetControllerType()
    {
        return EControllerType.NOTE;
    }





    void Awake()
    {
        pages = GetComponentsInChildren<NotePageBase>(includeInactive: true);

        Init();
    }

    void Init()
    {
        ActiveNextBtnAndPrevBtn(false, false);
        ActiveObjects(false);
        InitVariables();
        InitPageEnabled();
    }

    /// <summary>
    /// ���� �ʱ�ȭ (ù �� ���� ���ο� ���� ���۵� ������ ȣ���)
    /// </summary>
    void InitVariables()
    {
        dayText.text = "Day " + ++dayCount;
        isNewDay = true;
        pageNum = 0;
        notePages = GetNotePageArray();
    }

    /// <summary>
    /// ��Ʈ ������ �ʱ�ȭ
    /// </summary>
    void InitPageEnabled()
    {
        foreach (NotePageBase page in pages)
        {
            page.StopDialogue();
            page.gameObject.SetActive(false);
            //page.SetPageEnabled()�� ���� �� ����� ������ ����
        }
    }




    #region PageSetting
    /// <summary>
    /// ���� ���� �Ѿ �� ��Ʈ ������ ���� �Լ�
    /// </summary>
    /// <returns></returns>
    public void SetDiary(string _code, StructData _structData)
    {
        var nextDiaryData = App.instance.GetDataManager().diaryData[_code];
        //var nextDiary += .script;
        SetNote("result", true);
        if (nextDiaryData.IsSelectScript == 0)
        {
            //yes += Invoke(_structData.YesFunctionName);
            //yes += SetDiary(_structData.code += "_Enter");
            //no += Invoke(_structData.NoFunction);
            //no += SetDiary(_structData.code += "_Pass");
        }
    }

    public NotePageBase[] GetNotePageArray()
    {
        List<NotePageBase> todayPages = new List<NotePageBase>();
        foreach (NotePageBase page in pages)
        {
            if (page.GetPageEnableToday())
                todayPages.Add(page);
        }

        return todayPages.ToArray();
    }

    public void SetNote(string _noteType, bool _isActive)
    {
        if (_noteType == "result")
            pages[0].SetPageEnabled(_isActive);
        else if (_noteType == "select")
            pages[1].SetPageEnabled(_isActive);
    }
    #endregion




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

    public void OnAfterSelect()
    {
        notePages[pageNum].gameObject.SetActive(false);

        CloseNote();
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
    public bool GetNewDay()
    {
        return isNewDay;
    }
    #endregion
}
