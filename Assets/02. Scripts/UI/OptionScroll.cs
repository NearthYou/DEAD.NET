using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class OptionScroll : MonoBehaviour
{
    public RectTransform textRect;
    public RectTransform buttonBackRect;
    public ScrollRect scrollRect;

    void Start()
    {

        float textHeight = textRect.sizeDelta.y;

        // �� ��° �ؽ�Ʈ�� Y ��ǥ�� �����մϴ�.
        float buttonBackHeight = textHeight;
        buttonBackRect.anchoredPosition = new Vector2(buttonBackRect.anchoredPosition.x, buttonBackHeight);

        Canvas.ForceUpdateCanvases();
        scrollRect.normalizedPosition = Vector2.zero;
    }
}
