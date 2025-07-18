using System.Collections;
using UnityEngine;
using Yarn.Unity;

public class CustomYarnCommands : Singleton<CustomYarnCommands>
{
    [SerializeField] DialogueRunner dialogueRunner;

    void Awake()
    {
        //tutorial//
        dialogueRunner.AddCommandHandler<string, string>("highlight", HighLightObject);
        dialogueRunner.AddCommandHandler<string>("highlightBtn", HighLightBtn);
        dialogueRunner.AddCommandHandler<string>("waitUntil", WaitUntilUIState);
        dialogueRunner.AddCommandHandler("hide", HideDialogue);
        dialogueRunner.AddCommandHandler("show", ShowDialogue);
        dialogueRunner.AddCommandHandler<string>("setQuest", SetQuest);
        dialogueRunner.AddCommandHandler<bool>("setCloseBtnEnabled", SetCloseBtnEnabled);
        dialogueRunner.AddCommandHandler<int>("lightUpAndFillBattery", LightUpAndFillBattery);

        //01//
        dialogueRunner.AddCommandHandler("lightUpWorkBench", LightUpWorkBench);
        dialogueRunner.AddCommandHandler("lightDownWorkBench", LightDownWorkBench);

        //02//
        dialogueRunner.AddCommandHandler<string>("waitGetItem", WaitGetItem);

        //03//
        dialogueRunner.AddCommandHandler<string, bool>("setAlert", SetAlertState);
        dialogueRunner.AddCommandHandler("waitMoveScroll", WaitMoveScroll);
        dialogueRunner.AddCommandHandler<bool>("setScrollBar", SetScrollBar);

        //04//
        dialogueRunner.AddCommandHandler("lightUpMap", LightUpMap);
        dialogueRunner.AddCommandHandler("lightDownMap", LightDownMap);

        //05//
        dialogueRunner.AddCommandHandler("waitMovePoint", WaitMovePoint);
        dialogueRunner.AddCommandHandler("addResource", AddResource);

        //06//
        dialogueRunner.AddCommandHandler("waitNewDay", WaitNewDay);
        dialogueRunner.AddCommandHandler("enableBtn", EnableBtn);

        //08//
        dialogueRunner.AddCommandHandler("startPV", StartPV);
        dialogueRunner.AddCommandHandler("waitPVEnd", WaitPVEnd);
        dialogueRunner.AddCommandHandler("lightUp", LightUp);

        //09//
        dialogueRunner.AddCommandHandler("endTutorial", EndTutorial);

        //common//
        dialogueRunner.AddCommandHandler("appendNode", AppendNode);

        //dialogueRunner.AddCommandHandler("spawnTutorialGlicher", SpawnTutorialGlicher);
        //dialogueRunner.AddCommandHandler<string>("play_bgm", PlayBGM);
        //dialogueRunner.AddCommandHandler<string>("play_sfx", PlaySFX);
        //dialogueRunner.AddCommandHandler<string>("stop_bgm", StopBGM);
    }





    void AppendNode()
    {
        string nodeName = UIManager.instance.GetPageController().GetNextResourceNodeName();

        if (nodeName == "-1") return;

        UIManager.instance.GetPageController().CreateResultDialogueRunner(nodeName);
    }





    #region tutorial
    void HighLightObject(string _objectID, string _waitStatusName)
    {
        UIManager.instance.GetUIHighLightController().ShowHighLight(_objectID, _waitStatusName);
    }

    void HighLightBtn(string _objectID)
    {
        UIManager.instance.GetUIHighLightController().ShowBtnHighLight(_objectID);
    }

    Coroutine WaitUntilUIState(string _UIName)
    {
        return StartCoroutine(new WaitUntil(() => UIManager.instance.isUIStatus(_UIName)));
    }

    void HideDialogue()
    {
        TutorialManager.instance.GetTutorialController().Hide();
    }

    void ShowDialogue()
    {
        TutorialManager.instance.GetTutorialController().Show();
    }

    void SetQuest(string _questCode)
    {
        UIManager.instance.GetQuestController().CreateQuest(_questCode);
    }

    void SetCloseBtnEnabled(bool _isEnabled)
    {
        UIManager.instance.GetNoteController().SetCloseBtnEnabled(_isEnabled);
    }

    void LightUpAndFillBattery(int _num)
    {
        TutorialManager.instance.GetTutorialController().LightUpAndFillBattery(_num);
    }
    #endregion





    #region 01
    void LightUpWorkBench()
    {
        TutorialManager.instance.GetTutorialController().LightUpWorkBench();
    }

    void LightDownWorkBench()
    {
        TutorialManager.instance.GetTutorialController().LightDownWorkBench();
    }
    #endregion






    #region 02
    Coroutine WaitGetItem(string _itemCode)
    {
        return StartCoroutine(new WaitUntil(() => UIManager.instance.GetInventoryController().CheckInventoryItem(_itemCode)));
    }
    #endregion





    #region 03
    void SetAlertState(string _alertType, bool _isActive)
    {
        UIManager.instance.GetAlertController().SetAlert(_alertType, _isActive);
    }

    Coroutine WaitMoveScroll()
    {
        return StartCoroutine(new WaitUntil(() => UIManager.instance.GetNoteController().CheckIfScrolledToEnd()));
    }

    void SetScrollBar(bool _isInteractable)
    {
        UIManager.instance.GetNoteController().SetScrollBarInteractable(_isInteractable);
    }
    #endregion





    #region 04
    void LightUpMap()
    {
        TutorialManager.instance.GetTutorialController().LightUpMap();
    }

    void LightDownMap()
    {
        TutorialManager.instance.GetTutorialController().LightDownMap();
    }
    #endregion





    #region 05
    Coroutine WaitMovePoint()
    {
        return StartCoroutine(new WaitUntil(() => App.instance.GetMapManager().mapUIController.MovePointActivate()));
    }

    void AddResource()
    {
        TutorialManager.instance.GetTutorialController().AddSteel();
    }
    #endregion





    #region 06
    Coroutine WaitNewDay()
    {
        return StartCoroutine(new WaitUntil(() => UIManager.instance.GetNoteController().GetNewDay()));
    }

    void EnableBtn()
    {
        TutorialManager.instance.GetTutorialController().EnableBtn();
    }
    #endregion




    #region 08
    void StartPV()
    {
        UIManager.instance.GetPVController().Start01();
    }

    Coroutine WaitPVEnd()
    {
        return StartCoroutine(new WaitUntil(() => UIManager.instance.GetPVController().isEnd));
    }


    void LightUp()
    {
        TutorialManager.instance.GetTutorialController().LightUpBackground();
    }
    #endregion





    #region 09
    void EndTutorial()
    {
        TutorialManager.instance.EndTutorial();
    }
    #endregion





    #region temp
    void SpawnTutorialGlicher()
    {
        MapController.instance.SpawnTutorialZombie();
    }

    void PlayBGM(string bgmName)
    {
        App.instance.GetSoundManager().PlayBGM(bgmName);
    }

    void PlaySFX(string sfxName)
    {
        App.instance.GetSoundManager().PlaySFX(sfxName);
    }

    void StopBGM(string soundName)
    {
        App.instance.GetSoundManager().StopBGM();
    }
    #endregion

    [YarnFunction("getResourceName")]
    static string GetResourceName()
    {
        string resourceName = UIManager.instance.GetPageController().currResource;
        return resourceName;
    }

    [YarnFunction("getResourceIndex")]
    static int GetResourceIndex()
    {
        int resourceIndex = UIManager.instance.GetPageController().currResourceIndex;
        return resourceIndex;
    }


    [YarnFunction("getResourceCount")]
    static int GetResourceCount(string _itemCode)
    {
        var resources = App.instance.GetMapManager().resourceManager.GetLastResources();

        int count = 0;

        for (int i = 0; i < resources.Count; i++)
        {
            if (resources[i].ItemBase.data.Code == _itemCode)
                count = resources[i].ItemCount;
        }

        return count;
    }


    [YarnCommand("custom_wait")]
    static IEnumerator CustomWait(float _time)
    {
        yield return new WaitForSeconds(_time);
    }


    [YarnFunction("getStructName")]
    static string GetStructName()
    {
        string structName = UIManager.instance.GetPageController().currStruct;
        
        return structName;
    }
}