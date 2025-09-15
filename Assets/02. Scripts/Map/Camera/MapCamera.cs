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
        
        // Player가 생성될 때까지 대기
        await UniTask.WaitUntil(() => GameObject.FindGameObjectWithTag("Player") != null);
        
        player = GameObject.FindGameObjectWithTag("Player");
        
        // MapUi가 존재하는지 확인
        var mapUiObject = GameObject.FindGameObjectWithTag("MapUi");
        if (mapUiObject != null && mapUiObject.transform.childCount > 0)
        {
            mapUi = mapUiObject.transform.GetChild(0).gameObject;
        }
        
        // null 체크 후 카메라 설정
        if (player != null && mapCamera != null)
        {
            mapCamera.Follow = player.transform;
            mapCamera.m_Lens.OrthographicSize = 6.5f;
        }
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
