using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;
using static Yarn.Unity.Effects;
using Yarn.Unity;

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
        
    }

    public void EndTutorial()
    {
        Destroy(this);
    }
}
