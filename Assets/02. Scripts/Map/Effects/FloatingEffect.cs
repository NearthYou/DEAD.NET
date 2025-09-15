using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FloatingEffect : MonoBehaviour
{
    public IEnumerator FloatingAnimation()
    {
        while (true)
        {
            var tr = transform.position;
            tr.y = transform.parent.position.y + 0.2f + Mathf.Sin(Time.time)/5;
            transform.position = tr;
            yield return null;
        }
    }
}
