using DG.Tweening;
using UnityEngine;
using static NotePage;

public class MapInteraction : MonoBehaviour
{
    [SerializeField] Transform implant;

    Vector2 implantOriginalPos;

    void Start()
    {
        implantOriginalPos = implant.transform.position;

        //ImplantOpenEvent += ImplantOpenAnim;
        //ImplantCloseEvent += ImplantCloseAnim;
        App.instance.GetSoundManager().PlayBGM("BGM_InGameTheme");
    }

    /// <summary>
    /// ���ö�Ʈ ������ �ִϸ��̼�
    /// </summary>
    public void ImplantOpenAnim()
    {
        implant.DOMoveX(implantOriginalPos.x + 220f, 0.5f);
    }

    /// <summary>
    /// ���ö�Ʈ ������ �ִϸ��̼�
    /// </summary>
    public void ImplantCloseAnim()
    {
        implant.DOMoveX(implantOriginalPos.x, 0.5f);
    }
}
