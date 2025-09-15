using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public class BlinkEffect : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private float playTime = 0.75f;
    public bool isStop;
    
    private Sequence sequence;
    private MeshRenderer mesh;
    private Material material;
    private WaitForSeconds delayTime = new WaitForSeconds(0.75f);

    private void Awake()
    {
        mesh = GetComponent<MeshRenderer>();
        material = mesh.material;
    }

    private void OnEnable()
    {
        StartCoroutine(ChoiceAnimation());
    }

    public IEnumerator ChoiceAnimation()
    {
        if (isStop)
            yield return null;

        material.DOColor(Color.clear, playTime);
        yield return delayTime;
        material.DOColor(Color.white, playTime);
        yield return delayTime;

        StartCoroutine(ChoiceAnimation());
    }
}
