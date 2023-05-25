using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using UnityEngine.SceneManagement;
using DG.Tweening;
using System;

public class NoteController : MonoBehaviour
{
    [SerializeField] RectTransform noteCenterPos;
    [SerializeField] RectTransform noteRightPos;
    [SerializeField] RectTransform notePos;
    [SerializeField] GameObject pageContainer;
    [SerializeField] Transform[] notePages;

    [SerializeField] GameObject prefab;
    [SerializeField] Transform parent;

    [SerializeField] Button nextPageBtn;
    [SerializeField] Button prevPageBtn;
    [SerializeField] Button nextDayBtn;

    [SerializeField] DialogueRunner[] dialogueRunner;

    [SerializeField] GameObject[] optionButtons;

    string[] options;

    InMemoryVariableStorage variableStorage;

    bool newDay = true;
    int dialogueRunnerIndex = 0;
    string nodeName;
    
    public int pageNum = 0;
    int dayCount = 1;
    
    List<int> numbers = new List<int>() { 1, 2, 3, 4, 5 };
    int selectedNumber;

    void Start()
    {
        Transform[] pages = pageContainer.GetComponentsInChildren<Transform>();
        List<Transform> targets = new List<Transform>();
        foreach (Transform page in pages)
        {
            if (page.CompareTag("NotePage"))
            {
                targets.Add(page);
            }
        }

        notePages = targets.ToArray();

        CameraMove cameraMove = FindObjectOfType<CameraMove>();

        for (int i = 0; i < notePages.Length; i++)
        {
            notePages[i].gameObject.SetActive(false);
            var page = notePages[i].GetComponent<NotePage>();
            page.Init(cameraMove);

            if (page.isNoteMoveRight)
                page.pageOnEvent += MoveNoteRight;
            else
                page.pageOnEvent += MoveNoteCenter;
        }

        int randomIndex = UnityEngine.Random.Range(0, numbers.Count);
        selectedNumber = numbers[randomIndex];
        numbers.RemoveAt(randomIndex);

        InstantiateNewNameCard();

        nextPageBtn.onClick.AddListener(NextPageEvent);
        prevPageBtn.onClick.AddListener(PrevPageEvent);
        nextDayBtn.onClick.AddListener(NextDayEvent);

        options = new string[4];
        for (int i = 0; i < options.Length; i++)
            optionButtons[i].SetActive(false);
        //ChooseEventSetOption();
    }

    private void MoveNoteCenter()
    {
        notePos.DOAnchorPos(new Vector2(noteCenterPos.anchoredPosition.x, notePos.anchoredPosition.y), 1f);
    }

    private void MoveNoteRight()
    {
        notePos.DOAnchorPos(new Vector2(noteRightPos.anchoredPosition.x, notePos.anchoredPosition.y), 1f);
    }

    /// <summary>
    /// ���� ���� �� NoteAnim.cs���� ȣ��Ǵ� �Լ�
    /// </summary>
    public void OpenBox()
    {
        notePages[pageNum].gameObject.SetActive(true);
        if (newDay)
        {
            PageOn(0);
            newDay = false;
        }
        ChangePageButton();
    }
    /// <summary>
    /// ���� ���� �� NoteAnim.cs���� ȣ��Ǵ� �Լ� 
    /// </summary>
    public void CloseBox()
    {
        notePages[pageNum].gameObject.SetActive(false);
        nextPageBtn.gameObject.SetActive(false);
        prevPageBtn.gameObject.SetActive(false);
    }


    /// <summary>
    /// ���� ������ ��ư Ŭ�� �� ȣ��
    /// </summary>
    void NextPageEvent()
    {
        if (pageNum + 1 > notePages.Length - 1)
            return;

        ChangePage(pageNum + 1);
    }
    /// <summary>
    /// ���� ������ ��ư Ŭ�� �� ȣ��
    /// </summary>
    void PrevPageEvent()
    {
        if (pageNum - 1 < 0)
            return;

        ChangePage(pageNum - 1);
    }


    /// <summary>
    /// ����/���� �������� �̵�
    /// </summary>
    /// <param name="index"></param>
    public void ChangePage(int index)
    {
        notePages[pageNum].gameObject.SetActive(false);
        notePages[index].gameObject.SetActive(true);

        PageOn(index);
        pageNum = index;
        ChangePageButton();
    }
    /// <summary>
    /// �� ������ ������ yarn node �̵�
    /// </summary>
    /// <param name="index"></param>
    void PageOn(int index)
    {
        switch (index)
        {
            case 0:
                dialogueRunnerIndex = 0;
                nodeName = "Day" + dayCount;
                break;
            case 1:
                dialogueRunnerIndex = 1;
                nodeName = "Day" + dayCount + "ChooseEvent";
                break;
            case 4:
                dialogueRunnerIndex = 2;
                nodeName = "specialEvent" + selectedNumber;
                break;
            default:
                return;
        }

        if (!dialogueRunner[dialogueRunnerIndex].IsDialogueRunning)
            dialogueRunner[dialogueRunnerIndex].StartDialogue(nodeName);
    }


    /// <summary>
    /// ������ �̵� ��ư �̹��� ����
    /// </summary>
    void ChangePageButton()
    {
        if (pageNum == 0)
        {
            nextPageBtn.gameObject.SetActive(true);
            prevPageBtn.gameObject.SetActive(false);
        }
        else if (pageNum == notePages.Length - 1)
        {
            nextPageBtn.gameObject.SetActive(false);
            prevPageBtn.gameObject.SetActive(true);
            nextDayBtn.gameObject.SetActive(true);
        }
        else
        {
            nextPageBtn.gameObject.SetActive(true);
            prevPageBtn.gameObject.SetActive(true);
            nextDayBtn.gameObject.SetActive(false);
        }
    }


    /// <summary>
    /// ���� ��ư Ŭ�� �� �ϰ� ��Ʈ ���� �ʱ�ȭ
    /// </summary>
    void NextDayEvent()
    {
        pageNum = 0;
        for(int i = 0; i < dialogueRunner.Length; i++)
            dialogueRunner[i].Stop();
        int randomIndex = UnityEngine.Random.Range(0, numbers.Count);
        selectedNumber = numbers[randomIndex];
        numbers.RemoveAt(randomIndex);
        dayCount++;
        newDay = true;
        RemoveExistingNameCard();
        InstantiateNewNameCard();
        //ChooseEventSetOption();
    }
    /// <summary>
    /// ������ NameCard ������ ����
    /// </summary>
    void RemoveExistingNameCard()
    {
        notePages[1].gameObject.SetActive(true);
        GameObject[] existingPrefabs = GameObject.FindGameObjectsWithTag("NameCardPrefab");
        foreach (GameObject prefab in existingPrefabs)
        {
            Destroy(prefab);
        }
        notePages[1].gameObject.SetActive(false);
    }
    /// <summary>
    /// NameCard ������ ����
    /// </summary>
    void InstantiateNewNameCard()
    {
        for (int i = 0; i < selectedNumber; i++)
        {
            GameObject nameCard = Instantiate(prefab, parent);
            nameCard.tag = "NameCardPrefab";
        }
    }

    public void ChooseEventSetOption()
    {
        dialogueRunner[2].StartDialogue("Day" + dayCount + "ChooseEvent");
        variableStorage = GameObject.FindObjectOfType<InMemoryVariableStorage>();
        for (int i = 0; i < options.Length; i++)
        {
            variableStorage.TryGetValue("$option" + (i + 1), out options[i]);
            if (options[i] == "null")
                return;
            else
            {
                optionButtons[i].SetActive(true);
                optionButtons[i].GetComponent<Text>().text = options[i];
            }
        }
        dialogueRunner[2].Stop();
    }
}
