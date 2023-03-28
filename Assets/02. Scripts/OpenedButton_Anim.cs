using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class OpenedButton_Anim : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    private Text buttonText;
    private Image buttonImage;

    private Color normalTextColor = Color.cyan;
    private Color highlightTextColor = Color.white;

    private void Start()
    {
        // ���� ������Ʈ���� Text�� Image ������Ʈ�� �����ɴϴ�.
        buttonText = GetComponentInChildren<Text>();
        buttonImage = GetComponentInChildren<Image>();

        // �ʱ� ���¿����� Text�� ���� cyan�̰� Image�� �Ⱥ��Դϴ�.
        buttonText.color = normalTextColor;
        buttonImage.enabled = false;
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        // Highlighted ������ �� Text�� ���� ������� �ٲٰ� Image�� ���̰� �մϴ�.
        buttonText.color = highlightTextColor;
        buttonImage.enabled = true;
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        // Normal�� ���·� ���ư��� Text�� ���� cyan���� �ٲٰ� Image�� �Ⱥ��̰� �մϴ�.
        buttonText.color = normalTextColor;
        buttonImage.enabled = false;
    }
}