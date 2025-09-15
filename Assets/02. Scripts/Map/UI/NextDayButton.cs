using System.Collections;
using System.Collections.Generic;
using UnityEngine.UI;
using UnityEngine;

public class NextDayButton : MonoBehaviour
{
    MapController controller;
    void Start()
    {
        StartCoroutine(GetMapController());
    }

    IEnumerator GetMapController()
    {
        yield return new WaitForEndOfFrame();
        controller = GameObject.FindGameObjectWithTag("MapController").GetComponent<MapController>();
        gameObject.GetComponent<Button>().onClick.AddListener(NextDay);
    }

    void NextDay()
    {
        controller.NextDay();
    }
}
