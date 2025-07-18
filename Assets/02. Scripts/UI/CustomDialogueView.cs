using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Yarn.Unity;
using TMPro;
using System;

public class CustomDialogueView : DialogueViewBase
{
    public Button[] skipButton;
    [SerializeField] private TextMeshProUGUI lineText;
    [SerializeField] private string enterSFX = null;
    [SerializeField] private string textSFX = null;

    //private bool isRunningLine = false;
    private bool doesUserContinueRequest = false;
    private bool doesUserSkipRequest = false;
    private bool isStartLine = false;

    Effects.CoroutineInterruptToken typingCoroutineStopToken = new Effects.CoroutineInterruptToken();

    // 유저가 스킵 버튼 눌렀을 때 호출되는 함수
    public override void UserRequestedViewAdvancement()
    {
        if (!doesUserSkipRequest)
        {
            doesUserSkipRequest = true;

            if (typingCoroutineStopToken.CanInterrupt)
                typingCoroutineStopToken.Interrupt();

            lineText.maxVisibleCharacters = 10000;

            if (enterSFX != null) 
                App.instance.GetSoundManager().PlaySFX(enterSFX);
        }
        else
        {
            if (!doesUserContinueRequest && enterSFX != null)
                App.instance.GetSoundManager().PlaySFX(enterSFX);
            doesUserContinueRequest = true;
        }
    }

    public override void DialogueStarted()
    {
        for (int i = 0; i < skipButton.Length; i++) 
        {
            //skipButton[i].onClick.RemoveAllListeners();
            skipButton[i].onClick.AddListener(UserRequestedViewAdvancement);
        }
        
        //skipButton.gameObject.SetActive(true);
    }

    public override void DialogueComplete()
    {
        
    }

    public override void RunLine(LocalizedLine dialogueLine, Action onDialogueLineFinished)
    {
        //isRunningLine = true;

        StartCoroutine(UpdateLine(dialogueLine, onDialogueLineFinished));
    }

    private IEnumerator UpdateLine(LocalizedLine _localizedLine, Action _onDialogueLineFinished)
    {
        //if (_localizedLine.Metadata?.Length > 0)
        //{
        //    foreach (var metaData in _localizedLine.Metadata)
        //    {
        //        Debug.LogError($"Tag Detected : {metaData}");
        //        ReadYarnTag(metaData, _localizedLine.CharacterName);
        //    }
        //}

        //skipButton.gameObject.SetActive(true);

        doesUserSkipRequest = false;
        doesUserContinueRequest = false;
        isStartLine = true;

        lineText.text = _localizedLine.Text.Text;

        typingCoroutineStopToken = new Effects.CoroutineInterruptToken();


        yield return StartCoroutine(Effects.Typewriter(lineText, 30f, null, typingCoroutineStopToken)); // 타이핑 이펙트

        isStartLine = false;
        doesUserSkipRequest = true; // 유저 스킵 요청 불가(타이핑이 모두 완료되었으므로)

        yield return new WaitUntil(() => doesUserContinueRequest); // 유저가 다음으로 버튼 클릭할 때 까지 대기

        _onDialogueLineFinished?.Invoke();

        //isRunningLine = false;
    }

    void Update()
    {
        if (textSFX == null) return;
        if(!doesUserSkipRequest && isStartLine)
        {
            if (App.instance.GetSoundManager().CheckTypeWriteSFXPlayNow() == false) 
                App.instance.GetSoundManager().PlayTypeWriteSFX(textSFX);
        }
    }
}
