using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cysharp.Threading.Tasks;

public class FloatingEffect : MonoBehaviour
{
    public async UniTask FloatingAnimationAsync()
    {
        while (true)
        {
            var tr = transform.position;
            tr.y = transform.parent.position.y + 0.2f + Mathf.Sin(Time.time)/5;
            transform.position = tr;
            await UniTask.Yield();
        }
    }
}
