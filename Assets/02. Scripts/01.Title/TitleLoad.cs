using System.Collections;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Video;
using DG.Tweening;
using TMPro;

public class TitleLoad : MonoBehaviour
{
    [Header("Loading Objects")]
    [SerializeField] GameObject loadingVideo;
    VideoPlayer videoPlayer;

    [SerializeField] TextMeshProUGUI leftLogField;
    string leftFileText;
    [SerializeField] TextMeshProUGUI rightLogField;
    string rightFileText;
    string[] lines;

    [SerializeField] ScrollRect rightLogScrollRect;
    [SerializeField] float rightLogShowInterval;

    [Header("Title Objects")]
    [SerializeField] GameObject titleText;
    [SerializeField] GameObject titleImage;

    [SerializeField] GameObject buttonText;
    [SerializeField] GameObject buttonBack;






    void Awake()
    {
        Screen.SetResolution(1920, 1080, false);
    }

    void Start()
    {
        Init();
    }

    void Init()
    {
        App.instance.GetSoundManager().PlaySFX("SFX_Title_LogIn");

        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoEnd;

        string leftfilePath = "Text/Tittle_Log_LeftAccess";
        leftFileText = Resources.Load<TextAsset>(leftfilePath).text;

        string rightfilePath = "Text/Tittle_Log_RightLog";
        rightFileText = Resources.Load<TextAsset>(rightfilePath).text;

        lines = rightFileText.Split('\n');

        Screen.SetResolution(1920, 1080, false);

        InitObjects();
    }

    void InitObjects()
    {
        ActiveTitleObjects(false);
        ActiveButtonObjects(false);
    }





    void ActiveTitleObjects(bool _isActive)
    {
        titleText.SetActive(_isActive);
        titleImage.SetActive(_isActive);
    }

    void ActiveButtonObjects(bool _isActive)
    {
        buttonText.SetActive(_isActive);
        buttonBack.SetActive(_isActive);
    }





    /// <summary>
    /// ���� ����� ������ �� ��Ȱ��ȭ�ϴ� �Լ�
    /// </summary>
    /// <param name="vp"></param>
    void OnVideoEnd(VideoPlayer vp)
    {
        Screen.SetResolution(1920, 1080, false);
        loadingVideo.SetActive(false);
        Left();
    }





    /// <summary>
    /// ������� �α� ���
    /// </summary>
    void Left()
    {
        leftLogField.DOText(leftFileText, 2f)
            .SetEase(Ease.InSine)
            .OnComplete(() =>
            {
                StartCoroutine(RightLog());
            });
    }

    /// <summary>
    /// ������� �α� ���
    /// </summary>
    /// <returns></returns>
    IEnumerator RightLog()
    {
        int currentIndex = 0;
        rightLogField.text = "";

        while (currentIndex < lines.Length)
        {
            string line = lines[currentIndex];
            rightLogField.text += line + '\n';
            currentIndex++;

            rightLogScrollRect.verticalNormalizedPosition = 0.0f;

            yield return new WaitForSeconds(rightLogShowInterval);
        }

        StartCoroutine(Title());
    }





    /// <summary>
    /// Ÿ��Ʋ ���
    /// </summary>
    /// <returns></returns>
    IEnumerator Title()
    {
        yield return new WaitForSeconds(1f);

        ActiveTitleObjects(true);

        SetTitleText();
        SetTitleImage();

        App.instance.GetSoundManager().PlayBGM("BGM_TitleTheme");

        yield return new WaitForSeconds(2f);

        ActiveButtonObjects(true);
    }

    void SetTitleText()
    {
        TextMeshProUGUI text = titleText.GetComponent<TextMeshProUGUI>();

        text.alpha = 0f;
        text.DOFade(1f, 0f).SetEase(Ease.Linear);
    }

    void SetTitleImage()
    {
        Image title = titleImage.GetComponent<Image>();

        title.color = new Color(1f, 1f, 1f, 0f);
        title.DOFade(1f, 1f).SetEase(Ease.Linear);
    }
}
