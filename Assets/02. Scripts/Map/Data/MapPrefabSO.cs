using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

[System.Serializable]
public class MapPrefabs
{
    public GameObject prefab;
}

[CreateAssetMenu(fileName = "MapPrefabSO", menuName = "Scriptable Object/MapPrefabSO")]
public class MapPrefabSO : ScriptableObject
{
    public MapPrefabs[] items;
}