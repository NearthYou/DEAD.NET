using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Borders : MonoBehaviour
{
    [SerializeField] GameObject[] borders;
    [SerializeField] Material[] materials;
    [SerializeField] ETileState currentTileState;

    public GameObject GetNormalBorder()
    {
        currentTileState = ETileState.None;
        borders[0].GetComponent<MeshRenderer>().material = materials[0];
        return borders[0];
    }

    public GameObject GetAvailableBorder()
    {
        currentTileState = ETileState.Moveable;
        borders[0].GetComponent<MeshRenderer>().material = materials[1];
        return borders[0];
    }

    public GameObject GetUnAvailableBorder()
    {
        currentTileState = ETileState.Unable;
        borders[0].GetComponent<MeshRenderer>().material = materials[2];
        return borders[0];
    }
    
    public GameObject GetDisturbanceBorder()
    {
        currentTileState = ETileState.Target;
        return borders[1];
    }
    
    public void OffNormalBorder()
    {
        borders[0].SetActive(false);
    }
    
    public void OffTargetBorder()
    {
        borders[1].SetActive(false);
    }

    public void OffAllBorders()
    {
        for (int i = 0; i < borders.Length; i++)
        {
            GameObject item = borders[i];
            item.SetActive(false);
        }
    }

    public ETileState GetEtileState()
    {
        return currentTileState;
    }
}
