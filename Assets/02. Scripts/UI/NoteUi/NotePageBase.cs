using UnityEngine;

public abstract class NotePageBase : MonoBehaviour
{

    protected bool isNeedToday = true; //�ӽ÷� true�� default�� ����
    protected string nodeName;

    public abstract ENotePageType GetENotePageType();
    public abstract void PlayPageAction();
    public virtual void SetNodeName(string _nodeName)
    {
        this.nodeName = _nodeName;
    }

    public virtual void SetPageEnabled(bool _isNeedToday)
    {
        this.isNeedToday = _isNeedToday;
    }

    public virtual bool GetPageEnableToday()
    {
        return isNeedToday;
    }

    public abstract void StopDialogue();
}
