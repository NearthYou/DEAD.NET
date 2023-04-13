using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ClosedButton_Anim : MonoBehaviour
{
    public Button button;
    public Animator anim;
    public Animation OpenedAnim;
    public AnimationClip OpenedClip;
    public GameObject openedButton;

    private void Start()
    {
        openedButton = GameObject.Find("Opened_Button");
        openedButton.SetActive(false);

        this.anim.Play("Normal");

        button.onClick.AddListener(() =>
        {
            this.anim.speed = 1;
            this.anim.Play("Selected");
        });
    }

    /// <summary>
    /// ClosedButton_anim ���� �� Opened �ִϸ��̼� Ŭ�� ��� �̺�Ʈ ȣ��
    /// </summary>
    public void PlayOpenedAnimation()
    {
        openedButton.SetActive(true);
        OpenedAnim.clip = OpenedClip;
        OpenedAnim.Play();
    }
}
