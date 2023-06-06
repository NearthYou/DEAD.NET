using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;

class AutoResizeLineView : MonoBehaviour
{
    public void ResizeParent()
    {
        // �ڽ� ������Ʈ�� Renderer ������Ʈ�� �����ɴϴ�.
        RectTransform childRectTransform = transform.GetChild(0) as RectTransform;

        if (childRectTransform != null)
        {
            Debug.Log("childRectTransform != null");
            float childHeight = childRectTransform.sizeDelta.y;
            
            RectTransform parentRectTransform = GetComponent<RectTransform>();
            Vector2 newSize = childRectTransform.sizeDelta;
            newSize.y = childHeight;
            Debug.Log(childHeight);
            if (parentRectTransform != null)
            {
                parentRectTransform.sizeDelta = newSize;
            }
        }
    }
}

