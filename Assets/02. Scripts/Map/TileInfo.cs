using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TileInfo : MonoBehaviour
{
    /*    ETileType tileType;
        EWeatherType weatherType;
        TileInfo[] aroundTiles;
        string buildingID;
        int resourceID;
        string specialID;
        bool isCanMove;*/
    bool isDistrubtorOn;
    [SerializeField]TMP_Text resourceText;
    List<Resource> owendResources;

    public class Resource
    {
        public Resource(string type, int count)
        {
            this.type = type;
            this.count = count;
        }

        public string type;
        public int count;
    }

    private void Start()
    {
        owendResources = new List<Resource>();

        // Ÿ�� ���� ���� ���� + ������ Ȱ��ȭ
        ResourcesUpdate();
        TextUpdate();
    }

    void ResourcesUpdate()
    {
        string type = "";
        var random = Random.Range(1, 3);
        for (int i = 0; i < random; i++)
        {
            var randomType = Random.Range(1, 5);

            switch (randomType)
            {
                case 1:
                    type = "��";
                    break;
                case 2:
                    type = "�ķ�";
                    break;
                case 3:
                    type = "ö��";
                    break;
                case 4:
                    type = "����";
                    break;
            }

            var randomInt = Random.Range(1, 15);

            owendResources.Add(new Resource(type, randomInt));
        }


    }

    void TextUpdate()
    {
        if (owendResources.Count == 2)
            resourceText.text = "�ڿ� : " + owendResources[0].type + " " + owendResources[0].count + ", " + owendResources[1]?.type + " " + owendResources[1]?.count;
        else
            resourceText.text = "�ڿ� : " + owendResources[0].type + " " + owendResources[0].count;
    }

    public void DistrubtorOnOff(bool onoff)
    {
        isDistrubtorOn = onoff;
    }
}
