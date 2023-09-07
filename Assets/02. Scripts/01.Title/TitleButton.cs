using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    Text buttonText;
    Image buttonImage;

    Color normalTextColor = Color.white;
    Color highlightTextColor = Color.black;

    [SerializeField] GameObject titlePanel;
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject soundPanel;





    void Start()
    {
        buttonText = GetComponentInChildren<Text>();
        buttonImage = GetComponentInChildren<Image>();

        optionPanel.SetActive(false);
        soundPanel.SetActive(false);

        SetButtonNomal();
    }





    void SetButtonNomal()
    {
        buttonText.color = normalTextColor;
        buttonImage.enabled = false;
    }

    void SetButtonHighlighted()
    {
        buttonText.color = highlightTextColor;
        buttonImage.enabled = true;
    }





    public void OnPointerEnter(PointerEventData eventData)
    {
        SetButtonHighlighted();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetButtonNomal();
    }





    /// <summary>
    /// �ɼǹ�ư�� ������ ��
    /// </summary>
    public void OpenOptionPanel()
    {
        titlePanel.SetActive(false);
        optionPanel.SetActive(true);
    }

    /// <summary>
    /// �ɼ� ȭ�鿡�� Ÿ��Ʋ�� ���ư��� ��ư�� ������ ��
    /// </summary>
    public void OpenTitlePanel()
    {
        titlePanel.SetActive(true);
        optionPanel.SetActive(false);
    }





    /// <summary>
    /// �ɼ� ȭ�鿡�� ���� ���� ��ư�� ������ ��
    /// </summary>
    public void OpenSoundPanel()
    {
        soundPanel.SetActive(true);
    }
}