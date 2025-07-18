using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Chapter01_4 : QuestBase
{
    private readonly string thisCode = "chapter01_AccessSignal";
    private readonly EQuestType thisType = EQuestType.Main;
    private readonly int thisIndex = 3;
    private readonly int thisNextIndex = 4;

    public Chapter01_4()
    {
        questCode = thisCode;
        eQuestType = thisType;
        questIndex = thisIndex;
        nextQuestIndex = thisNextIndex;
    }
    public override IEnumerator CheckQuestComplete()
    {
        yield return new WaitUntil(() => CheckMeetCondition());
        yield return new WaitUntil(() => UIManager.instance.isUIStatus("UI_NORMAL"));
        AfterQuest();
    }

    public override bool CheckMeetCondition()
    {
        return App.instance.GetMapManager().SensingSignalTower();
    }

    public override string SetQuestText()
    {
        return "송신 탑 찾기";
    }
}
