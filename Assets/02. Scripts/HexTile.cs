using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public HexTile[] neighbours; //������ ���Ÿ�� ��ü �迭

    public enum TerrainType{ Grassland, Machine, Desert, Water }; //Ÿ�� Ÿ��
    public enum TileHeight{ Flat, Uphill, Downhill }; //���� Ÿ��

    protected TerrainType terrainType;
    protected TileHeight tileHeight;
    protected bool moveAble; //�̵� ���� ����
    protected GameObject deck; //Ÿ�� ���� ��� �Ǵ� ������

    public virtual bool IsPassable()
    {
        return moveAble;
    }

    protected Color defaultColor; //�⺻ ����
    protected Color selectedColor = Color.yellow; //���õ��� �� ����

    void Start()
    {
        defaultColor = GetComponent<Renderer>().material.color;
    }
    public virtual void OnTileClicked() //Ÿ���� ���õ��� ��
    {
        GetComponent<Renderer>().material.color = selectedColor;
    }
}
