using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class UIManager : Singleton<UIManager>
{
    [SerializeField] NoteController noteController;
    [SerializeField] InventoryController inventoryController;
    [SerializeField] CraftingUiController craftingUiController;
    [SerializeField] CraftingRawImageController craftingRawImageController;
    [SerializeField] CraftModeController craftModeController;
    [SerializeField] UIHighLightController uiHighLightController;
    [SerializeField] SelectController selectController;
    [SerializeField] NextDayController nextDayController;
    [SerializeField] AlertController alertController;
    [SerializeField] MenuController menuController;
    [SerializeField] PageController pageController;
    [SerializeField] QuestController questController;
    [SerializeField] SoundController soundController;
    [SerializeField] ItemInfoController itemInfoController;
    [SerializeField] UpperController upperController;
    [SerializeField] PopUpController popUpController;
    [SerializeField] InfoController infoController;
    [SerializeField] PVController pvController;


    public Stack<string> currUIStack = new Stack<string>();

    private void Awake()
    {
        currUIStack.Push(StringUtility.UI_NORMAL);
    }
    void Update()
    {
        InputKey();
    }

    public void InputKey()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            if (isUIStatus("UI_MENU") == false)
                menuController.EnterMenu();
            else
                menuController.QuitMenu();
        }
    }

    public void AddCurrUIName(string _uiName)
    {
        currUIStack.Push(_uiName);

        //Debug.LogError("currUIStack : " + currUIStack.Peek());
    }

    public void PopCurrUI()
    {
        currUIStack.Pop();
    }

    public bool isUIStatus(string _cmp)
    {
        currUIStack.TryPeek(out string top);
        return _cmp == top;
    }

    public NoteController GetNoteController()
    {
        return noteController;
    }

    public InventoryController GetInventoryController()
    {
        return inventoryController;
    }

    public CraftingUiController GetCraftingUiController()
    {
        return craftingUiController;
    }

    public CraftingRawImageController GetCraftingRawImageController()
    {
        return craftingRawImageController;
    }

    public CraftModeController GetCraftModeController()
    {
        return craftModeController;
    }
   
    public UIHighLightController GetUIHighLightController()
    {
        return uiHighLightController;
    }

    public SelectController GetSelectController()
    {
        return selectController;
    }

    public NextDayController GetNextDayController()
    {
        return nextDayController;
    }

    public AlertController GetAlertController()
    {
        return alertController;
    }

    public MenuController GetMenuController()
    {
        return menuController;
    }

    public PageController GetPageController()
    {
        return pageController;
    }

    public QuestController GetQuestController()
    {
        return questController;
    }

    public SoundController GetSoundController()
    {
        return soundController;
    }

    public ItemInfoController GetItemInfoController()
    {
        return itemInfoController;
    }

    public UpperController GetUpperController()
    {
        return upperController;
    }

    public PopUpController GetPopUpController()
    {
        return popUpController;
    }

    public InfoController GetInfoController()
    {
        return infoController;
    }

    public PVController GetPVController()
    {
        return pvController;
    }
}
