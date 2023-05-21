using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class NotePage : MonoBehaviour
{
    /// <summary>
    /// �������� ������ �� ������ �̺�Ʈ
    /// </summary>
    public Action pageOnEvent;
    public ENotePageType notePageType;
    public bool isNoteMoveRight;

    CameraMove cameraMove;

    public void Init(CameraMove _cameramove)
    {
        cameraMove = _cameramove;
        pageOnEvent += () => { cameraMove.ChangeCamera(notePageType); };
    }

    private void OnEnable()
    {
        pageOnEvent?.Invoke();
    }
}
