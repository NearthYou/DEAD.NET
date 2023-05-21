using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using UnityEngine.SceneManagement;
using DG.Tweening;

public class NoteController : MonoBehaviour
{
    [SerializeField] RectTransform noteCenterPos;
    [SerializeField] RectTransform noteRightPos;
    [SerializeField] RectTransform notePos;
    [SerializeField] GameObject pageContainer;
    [SerializeField] GameObject dialogueBox;
    [SerializeField] GameObject nextDay;
    [SerializeField] Sprite[] btnImages;
    [SerializeField] Transform[] notePages;

    [SerializeField] GameObject prefab;
    [SerializeField] Transform parent;

    [SerializeField] Button nextPageBtn;
    [SerializeField] Button prevPageBtn;
    [SerializeField] Button nextDayBtn;

    [SerializeField] DialogueRunner dialogueRunner;
    InMemoryVariableStorage variableStorage;
    bool isEnd = false;
    bool isContinued = false;
    string nextNode;
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

        dialogueBox.SetActive(false);

        int randomIndex = Random.Range(0, numbers.Count);
        selectedNumber = numbers[randomIndex];
        numbers.RemoveAt(randomIndex);

        InstantiateNewNameCard();

        nextPageBtn.onClick.AddListener(NextPageEvent);
        prevPageBtn.onClick.AddListener(PrevPageEvent);
        nextDayBtn.onClick.AddListener(NextDayEvent);

        dialogueRunner.StartDialogue("Day1");
        variableStorage = GameObject.FindObjectOfType<InMemoryVariableStorage>();
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
        ChangePageButton();
        PageOn(pageNum);
    }
    /// <summary>
    /// ���� ���� �� NoteAnim.cs���� ȣ��Ǵ� �Լ� 
    /// </summary>
    public void CloseBox()
    {
        notePages[pageNum].gameObject.SetActive(false);
        dialogueBox.SetActive(false);
        nextPageBtn.image.sprite = btnImages[0];
        prevPageBtn.image.sprite = btnImages[0];
    }


    /// <summary>
    /// ���� ������ ��ư Ŭ�� �� ȣ��
    /// </summary>
    void NextPageEvent()
    {
        if (pageNum + 1 > notePages.Length - 1)
            return;

        if(isEnd || pageNum == 1 || pageNum == 2 || pageNum == 5)
        {
            dialogueBox.SetActive(false);
            ChangePage(pageNum + 1);
        }
  
        PageOn(pageNum);
    }
    /// <summary>
    /// ���� ������ ��ư Ŭ�� �� ȣ��
    /// </summary>
    void PrevPageEvent()
    {
        if (pageNum - 1 < 0)
            return;

        if (isEnd || pageNum == 1 || pageNum == 2 || pageNum == 5)
        {
            dialogueBox.SetActive(false);
            ChangePage(pageNum - 1);
        }
        
        PageOn(pageNum);
    }


    /// <summary>
    /// ����/���� �������� �̵�
    /// </summary>
    /// <param name="index"></param>
    public void ChangePage(int index)
    {
        notePages[pageNum].gameObject.SetActive(false);
        notePages[index].gameObject.SetActive(true);

        pageNum = index;
        isEnd = false;
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
                nodeName = "Day" + dayCount; 
                break;
            case 3:
                nodeName = "Day" + dayCount + "ChooseEvent"; 
                break;
            case 4:
                nodeName = "specialEvent" + selectedNumber; 
                break;
            default:
                return;
        }


        if (!isContinued)
        {
            dialogueBox.SetActive(true);
            ChangeNode(nodeName);
        }
        else
        {
            nodeName = nextNode;
            ChangeNode(nodeName);
        }
    }
    /// <summary>
    /// PageOn �Լ� ���� �ߺ��Ǵ� �κ� ���� �� �Լ�. ��ü���� ����� ��带 �����ϰ� �ش� ����� ������ �޾ƿ�.
    /// </summary>
    /// <param name="nodeName"></param>
    void ChangeNode(string nodeName)
    {
        dialogueRunner.Stop();
        dialogueRunner.StartDialogue(nodeName);
        variableStorage = GameObject.FindObjectOfType<InMemoryVariableStorage>();
        variableStorage.TryGetValue("$isContinued", out isContinued);
        variableStorage.TryGetValue("$nextNode", out nextNode);
        variableStorage.TryGetValue("$isEnd", out isEnd);
    }


    /// <summary>
    /// ������ �̵� ��ư �̹��� ����
    /// </summary>
    void ChangePageButton()
    {
        if (pageNum == 0)
        {
            nextPageBtn.image.sprite = btnImages[1];
            prevPageBtn.image.sprite = btnImages[0];
        }
        else if (pageNum == notePages.Length - 1)
        {
            nextPageBtn.image.sprite = btnImages[0];
            prevPageBtn.image.sprite = btnImages[1];
            nextDay.SetActive(true);
        }
        else
        {
            nextPageBtn.image.sprite = btnImages[1];
            prevPageBtn.image.sprite = btnImages[1];
            nextDay.SetActive(false);
        }
    }


    /// <summary>
    /// ���� ��ư Ŭ�� �� �ϰ� ��Ʈ ���� �ʱ�ȭ
    /// </summary>
    void NextDayEvent()
    {
        pageNum = 0;
        int randomIndex = Random.Range(0, numbers.Count);
        selectedNumber = numbers[randomIndex];
        numbers.RemoveAt(randomIndex);
        dayCount++;
        RemoveExistingNameCard();
        InstantiateNewNameCard();
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
}
