using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region ����ü
/// <summary>
/// �������� �����ִ� ����ü
/// ����, ���, ü��, ����� ����, �񸶸� ����, ����, ���, ���� ����
/// </summary>
public struct Data
{
    float attack;
    float defense;
    float health;
    float hungerFigure;
    float thirstFigure;
    float reliability;
    float battery;

    string name;
    string job;
    string[] equipments;

    bool isDead;
}

/// <summary>
/// Enum���� �����ִ� ����ü
/// </summary>
public struct Enums
{
    EHungerType eHunger;
    EThirstType eThirst;
    EConditionType eCondition;
    EBodyHealthType eBody;
    EInfectionType eInfection;
    EPartsType eParts;
} 
#endregion

public abstract class Character : MonoBehaviour
{
    Data data;
    Enums enums;

    /// <summary>
    /// �ʱ�ȭ �Լ�. Start���� ȣ��
    /// </summary>
    public abstract void InitSetting();

    /// <summary>
    /// ĳ������ ���º�ȭ�� ������Ʈ �����ִ� �Լ�. Update���� ȣ��
    /// </summary>
    public abstract void StateUpdate();

    public virtual void Start()
    {
        InitSetting();
    }

    public virtual void Update()
    {
        StateUpdate();
    }
}