using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class TutorialManager : Singleton<TutorialManager>
{
    // Ʃ�丮�� �� ���� ��ũ��Ʈ

    [Header("Tutorial")]
    [SerializeField]  TutorialController tutorialController;

    public Image lightBackground;
    public bool isLightUp = false;

    public TutorialController GetTutorialController()
    {
        return tutorialController;
    }

    void Awake()
    {
        lightBackground = GameObject.FindWithTag("LightImage").GetComponent<Image>();
        tutorialController.Init();
    }

    public void LightUpBackground()
    {
        lightBackground.DOFade(0f, 2f).SetEase(Ease.InBounce).OnComplete(() =>
        {
            lightBackground.gameObject.SetActive(false);
            isLightUp = true;
        });
    }

    public void EndTutorial()
    {
        Destroy(this);
    }
}
