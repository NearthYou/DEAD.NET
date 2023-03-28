using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class HexTile : MonoBehaviour
{
    public HexTile[] neighbours; //������ ���Ÿ�� ��ü �迭

    protected TerrainType terrainType;
    protected TileHeight tileHeight;
    protected bool moveAble; //�̵� ���� ����
    protected GameObject deck; //Ÿ�� ���� ��� �Ǵ� ������
    protected Color defaultColor; //�⺻ ����
    protected Color selectedColor = Color.yellow; //���õ��� �� ����

    void Start()
    {
        defaultColor = GetComponent<Renderer>().material.color;
    }

    /// <summary>
    /// �̵� ���� ���� ��ȯ
    /// </summary>
    public virtual bool IsPassable()
    {
        return moveAble;
    }

    /// <summary>
    /// Ÿ���� ���õ��� �� Ÿ�� ���� ����
    /// </summary>
    public virtual void OnTileClicked()
    {
        GetComponent<Renderer>().material.color = selectedColor;
    }
}
