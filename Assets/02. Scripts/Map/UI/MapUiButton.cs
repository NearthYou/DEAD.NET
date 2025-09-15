using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapUiButton : MonoBehaviour
{
    MapCamera mapCamera;
    // Start is called before the first frame update
    void Start()
    {
        StartCoroutine(GetMapCamera());
    }

    private IEnumerator GetMapCamera()
    {
        yield return new WaitForEndOfFrame();
        mapCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<MapCamera>();
    }

    // Update is called once per frame
/*    public void ChangeMapCamera(bool button)
    {
        mapCamera.SetPrioryty(button);
    }*/
}
