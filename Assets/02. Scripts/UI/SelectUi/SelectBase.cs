using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public abstract class SelectBase : MonoBehaviour
{
    public string key;

    public abstract void SelectA();
    public abstract void SelectB();
    public abstract void SetOptionA(Button _button);
    public abstract void SetOptionB(Button _button);
}
