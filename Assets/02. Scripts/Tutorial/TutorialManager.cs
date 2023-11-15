using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    // Ʃ�丮�� �� ���� ��ũ��Ʈ

    [Header("Tutorial")]
    [SerializeField]  TutorialController tutorialController;

    public TutorialController GetTutorialController()
    {
        return tutorialController;
    }

    public void StartTutorial()
    {
        tutorialController.LightDownBackground();
    }

    public void EndTutorial()
    {
        Destroy(this);
    }
}
