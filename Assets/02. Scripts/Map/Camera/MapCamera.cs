using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using Cysharp.Threading.Tasks;

public class MapCamera : MonoBehaviour
{
    [Header("References")]
    private GameObject player;
    private GameObject noteUi;
    private GameObject mapUi;
    public CinemachineVirtualCamera mapCamera;

    public async UniTask GetMapInfoAsync()
    {
        await UniTask.WaitForEndOfFrame(this);
        player = GameObject.FindGameObjectWithTag("Player");
        mapUi = GameObject.FindGameObjectWithTag("MapUi").transform.GetChild(0).gameObject;
        mapCamera.Follow = player.transform;
        mapCamera.m_Lens.OrthographicSize = 6.5f;
    }

    public void SetPrioryty(bool isOn)
    {
        if (isOn)
        {
            mapCamera.Priority = 11;
            UIManager.instance.AddCurrUIName(StringUtility.UI_MAP);
        }
        else
        {
            mapCamera.Priority = 8;
            UIManager.instance.PopCurrUI();
        }
        mapUi.SetActive(isOn);
    }
}
