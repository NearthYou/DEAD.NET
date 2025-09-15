using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class NextDayButton : MonoBehaviour
{
    MapController controller;
    void Start()
    {
        GetMapControllerAsync().Forget();
    }

    async UniTask GetMapControllerAsync()
    {
        await UniTask.WaitForEndOfFrame(this);
        controller = GameObject.FindGameObjectWithTag("MapController").GetComponent<MapController>();
        gameObject.GetComponent<Button>().onClick.AddListener(NextDay);
    }

    void NextDay()
    {
        controller.NextDayAsync().Forget();
    }
}
