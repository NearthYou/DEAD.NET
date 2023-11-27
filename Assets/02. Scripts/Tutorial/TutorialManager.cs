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
        UIManager.instance.GetPageController().SetTutorialSelect();

        UIManager.instance.GetAlertController().SetAlert("note", false);

        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_PLASMA");
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_CARBON");
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_STEEL");

        tutorialController.StartDialogue();
    }

    public void EndTutorial()
    {
        UIManager.instance.GetQuestController().CreateQuest("chapter01_GetNetworkChip"); //�ϴ� �ӽ�.. pv������ �װ� ��� ������ �����ǰ� �����ϰڽ��ϴ�.
        Destroy(this);
    }
}
