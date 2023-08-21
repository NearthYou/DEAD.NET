using System;
using UnityEngine;

public class NotePage : MonoBehaviour
{
    /// <summary>
    /// �������� ������ �� ������ �̺�Ʈ
    /// </summary>
    public Action pageOnEvent;
    public ENotePageType notePageType { get; private set; }
    public bool isNoteMoveRight { get; private set; }

    CameraMove cameraMove;

    public void Init(CameraMove _cameramove, ENotePageType _notePageType, bool _isNoteMoveRight)
    {
        cameraMove = _cameramove;
        notePageType = notePageType;
        isNoteMoveRight = isNoteMoveRight;
        pageOnEvent += () => { cameraMove.ChangeCamera(notePageType); };
    }

    private void OnEnable()
    {
        pageOnEvent?.Invoke();
    }
}
