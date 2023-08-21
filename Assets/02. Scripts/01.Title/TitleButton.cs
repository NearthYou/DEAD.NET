using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class TitleButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Text buttonText;
    private Image buttonImage;

    private Color normalTextColor = Color.white;
    private Color highlightTextColor = Color.black;

    [SerializeField] GameObject titlePanel;
    [SerializeField] GameObject optionPanel;
    [SerializeField] GameObject soundPanel;

    private void Start()
    {
        buttonText = GetComponentInChildren<Text>();
        buttonImage = GetComponentInChildren<Image>();

        buttonText.color = normalTextColor;
        buttonImage.enabled = false;
    }

    /// <summary>
    /// Highlighted ������ ��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerEnter(PointerEventData eventData)
    {
        buttonText.color = highlightTextColor;
        buttonImage.enabled = true;
    }

    /// <summary>
    /// Normal ���·� ���ư��� ��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerExit(PointerEventData eventData)
    {
        buttonText.color = normalTextColor;
        buttonImage.enabled = false;
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