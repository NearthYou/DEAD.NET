using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class TargetPointUI : MonoBehaviour
{
    Transform target = null;
    bool isOn;

    private void LateUpdate()
    {
        if(isOn)
            transform.position = Camera.main.WorldToScreenPoint(target.position);
    }

    public void OnEffect(Transform target)
    {
        isOn = true;
        this.target = target;
        //gameObject.SetActive(true);
        transform.position = Camera.main.WorldToScreenPoint(target.position);
    }

    public void OffEffect()
    {
        isOn = false;
        //gameObject.SetActive(false);
    }

    public bool ActivateStatus()
    {
        return isOn;
    }
}
