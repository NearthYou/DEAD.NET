using System.Collections;
using System.Collections.Generic;
using UnityEngine;

#region Enum
enum Food
{
    Normal,
    Hunger,
    Starving
}

enum Water
{
    Normal,
    Thirst,
    Dehydration
}

enum Condition
{
    Normal,
    Anx,
    Crazy,
    Machine
}

enum BodyHealth
{
    Normal,
    Hurt,
    Injury,
    Disease
}

enum Infection
{
    Normal,
    Bite,
    Zombie
}

enum Parts
{
    None,
    Eyeball,
    LeftArm,
    RightArm,
    Body,
    Legs
}
#endregion

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

    string job;
    string[] equipments;

    bool isDead;
}

/// <summary>
/// �ʿ��� ��� Enum���� �����ִ� ����ü
/// </summary>
public struct Enums
{
    Food food;
    Water water;
    BodyHealth bodyhealth;
    Infection infection;
    Condition condition;
} 
#endregion

public abstract class Character : MonoBehaviour
{
    public Data data;
    public Enums enums;
    
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