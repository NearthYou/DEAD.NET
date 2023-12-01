using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class NextDayBtn : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
{
    [Header("Gauage Obejcts")]
    [SerializeField] float fillSpeed = 1.0f;
    [SerializeField] float maxGaugeValue = 100.0f;
    float currentGaugeValue = 0.0f;
    bool isFilling = false;

    void Start()
    {
        InitGauageUI();
    }

    void Update()
    {
        if (isFilling) //��ư ������ ����
        {
            FillGauge();
            if (currentGaugeValue >= maxGaugeValue) //�������� �� ���� ���� ���� �̵�
            {
                InitGauageUI();
                //UIManager.instance.GetNextDayController().NextDayEvent();
            }
        }
    }

    /// <summary>
    /// ��ư ������ �ʱ�ȭ
    /// </summary>
    void InitGauageUI()
    {
        isFilling = false;
        currentGaugeValue = 0.0f;
    }

    /// <summary>
    /// ��ư�� ������ ���� ��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerDown(PointerEventData eventData)
    {
        isFilling = true;
    }

    /// <summary>
    /// ��ư���� ���� ��
    /// </summary>
    /// <param name="eventData"></param>
    public void OnPointerUp(PointerEventData eventData)
    {
        isFilling = false;
        InitGauageUI();
    }

    /// <summary>
    /// ������ ä��
    /// </summary>
    void FillGauge()
    {
        currentGaugeValue += fillSpeed * Time.deltaTime;
    }
}
