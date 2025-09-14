using System.Collections;
using UnityEngine;

public class TutorialManager : Singleton<TutorialManager>
{
    [Header("Tutorial")]
    [SerializeField] TutorialController tutorialController;

    public TutorialController GetTutorialController()
    {
        return tutorialController;
    }

    void Start()
    {
        StartCoroutine(WaitForMapManager());
    }

    IEnumerator WaitForMapManager()
    {
        yield return new WaitUntil(() => App.instance.GetMapManager().mapController);
        yield return new WaitUntil(() => App.instance.GetMapManager().mapController.Player != null);

        StartTutorial();
    }

    public void StartTutorial()
    {
        if (GameManager.instance.ShouldSkipTutorial())
        {
            SkipTutorialAndStartMain();
            return;
        }

        UIManager.instance.GetPageController().SetTutorialSelect();
        UIManager.instance.GetCraftingUiController().AddBatteryCombine();
        UIManager.instance.GetAlertController().SetAlert("note", false);
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_PLASMA");
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_CARBON");
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_STEEL");
        tutorialController.StartDialogue();
    }

    public void EndTutorial()
    {
        UIManager.instance.GetCraftingUiController().RemoveBatteryCombine();
        UIManager.instance.GetQuestController().StartMainQuest();
        Destroy(this);
    }

    private void SkipTutorialAndStartMain()
    {
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_PLASMA");
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_CARBON");
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_STEEL");
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_DISTURBE");
        UIManager.instance.GetInventoryController().AddItemByItemCode("ITEM_FINDOR");
        UIManager.instance.GetQuestController().StartMainQuest();
        Destroy(this);
    }
}