using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;
using Cysharp.Threading.Tasks;

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
        ChoiceAnimationAsync().Forget();
    }

    public async UniTask ChoiceAnimationAsync()
    {
        if (isStop)
            return;

        material.DOColor(Color.clear, playTime);
        await UniTask.Delay((int)(playTime * 1000));
        material.DOColor(Color.white, playTime);
        await UniTask.Delay((int)(playTime * 1000));

        ChoiceAnimationAsync().Forget();
    }
}
