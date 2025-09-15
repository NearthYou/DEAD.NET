using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class RawImageAnimation : MonoBehaviour
{
    Sequence sequence;

    // Start is called before the first frame update
    void Start()
    {
        sequence = DOTween.Sequence();
        sequence.AppendInterval(1f)
            .Append(GetComponent<RawImage>().DOFade(100, 1f));
    }

    private void OnDisable()
    {
        GetComponent<RawImage>().color = new Color32(255, 255, 255, 0);
    }

}
