using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class Alarm : MonoBehaviour, IPointerClickHandler
{
    [SerializeField] EAlarmType alarmType;

    [Header("TempForTest")] //�׽�Ʈ�� �ӽ� ������
    [SerializeField] GameObject NewAlarm;
    [SerializeField] GameObject ResultAlarm;
    [SerializeField] GameObject CautionAlarm;

    public void OnPointerClick(PointerEventData eventData)
    {
        if (this.alarmType == EAlarmType.New)
            EnableAlarm(ENotePageType.Select);
        else if (this.alarmType == EAlarmType.Result)
            EnableAlarm(ENotePageType.Result);
        else if (this.alarmType == EAlarmType.Caution)
            GameManager.instance.SetPrioryty(true);
    }

    void EnableAlarm(ENotePageType type)
    {
        UIManager.instance.AddCurrUIName(StringUtility.UI_NOTE);
        UIManager.instance.GetNoteController().SetPageNum(type);
        UIManager.instance.GetNoteController().OpenNote();
    }

    void SetAlarm(EAlarmType type) //�ϴ� �ӽ÷� ����.. ������ �����ؼ� �ش� �˸� ������ ���� ���� �� �� �Լ��� �˸� ȣ��
    {
        if (this.alarmType == type)
            gameObject.SetActive(true);
    }

    //�׽�Ʈ�� �ӽ� �Լ���
    #region Temp
    public void AddNew()
    {
        NewAlarm.SetActive(true);
    }
    public void RemoveNew()
    {
        NewAlarm.SetActive(false);
    }
    public void AddResult()
    {
        ResultAlarm.SetActive(true);
    }
    public void RemoveResult()
    {
        ResultAlarm.SetActive(false);
    }
    public void AddCaution()
    {
        CautionAlarm.SetActive(true);
    }
    public void RemoveCaution()
    {
        CautionAlarm.SetActive(false);
    }
    #endregion
}
