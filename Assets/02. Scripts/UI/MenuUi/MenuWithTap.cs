using UnityEngine;
using UnityEngine.UI;
using TMPro;
using DG.Tweening;

public class MenuWithTap : MenuButtonBase
{
    GameObject backBtn;
    [SerializeField] GameObject detailsBack;

    int hierarchyIndex;

    public override void Set()
    {
        buttonText = GetComponentInChildren<TextMeshProUGUI>();
        buttonImage = GetComponent<Image>();

        backBtn = transform.Find("Back_Btn").gameObject;

        hierarchyIndex = transform.GetSiblingIndex();
    }
    void SetChildrenStatus(bool _isOpen)
    {
        isClicked = _isOpen;
        backBtn.SetActive(_isOpen);
        detailsBack.SetActive(_isOpen);
    }

    public override void Init()
    {
        SetButtonNormal();
        SetChildrenStatus(false);
    }

    public override void ClickEvent()
    {
        transform.SetAsLastSibling();
        gameObject.GetComponent<Transform>().DOLocalMoveY(209f, 0.3f).OnComplete(() =>
        {
            SetChildrenStatus(true);
            SetButtonHighlighted();
        });
    }

    public override void CloseEvent()
    {
        SetChildrenStatus(false);
        gameObject.GetComponent<Transform>().DOLocalMoveY(initialY, 0.3f).OnComplete(() =>
        {
            transform.SetSiblingIndex(hierarchyIndex);
            isClicked = false;
            SetButtonNormal();
        });
    }
}
