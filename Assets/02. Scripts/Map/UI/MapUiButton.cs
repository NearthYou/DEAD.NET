using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class MapUiButton : MonoBehaviour
{
    MapCamera mapCamera;
    // Start is called before the first frame update
    void Start()
    {
        GetMapCameraAsync().Forget();
    }

    private async UniTask GetMapCameraAsync()
    {
        await UniTask.WaitForEndOfFrame(this);
        mapCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<MapCamera>();
    }

    // Update is called once per frame
/*    public void ChangeMapCamera(bool button)
    {
        mapCamera.SetPrioryty(button);
    }*/
}
