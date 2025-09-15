using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;

public enum ETileInfoTMP
{
    Landform,
    Zombie,
    Resource
}

public class TileInfoPanel : MonoBehaviour
{
    public Image illustration;
    public TMP_Text[] TMPs;

    public void UpdateImage(Sprite sprite)
    {
        illustration.sprite = sprite;
    }

    public void UpdateText(ETileInfoTMP infoTMP, string text)
    {
        TMPs[(int)infoTMP].text = text;
    }
}
