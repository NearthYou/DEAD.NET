using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MapManager : ManagementBase
{
    Camera mapCamera;
    MapController mapGenerator;
    MapUiController mapUiController;
    ResourceManager resourceManager;

    // Player ��ũ��Ʈ�� �ű��
    int currentHealth;
    int maxHealth = 1;

    IEnumerator GetAdditiveSceneObjects()
    {
        yield return new WaitForEndOfFrame();

    }

    public void CreateMapScene()
    {
        StartCoroutine(GetAdditiveSceneObjects());
    }

    public override EManagerType GetManagemetType()
    {
        throw new System.NotImplementedException();
    }
}
