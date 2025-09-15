using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapTestManager : Singleton<MapTestManager>
{
    [SerializeField] MapManager manager;

    public void TestStart()
    {
        manager.GetAdditiveSceneObjectsCoroutine();
    }
}
