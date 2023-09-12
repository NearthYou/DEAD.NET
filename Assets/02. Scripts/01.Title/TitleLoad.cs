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

    [SerializeField] ScrollRect scrollRect;

    [SerializeField] float leftLogShowInterval;
    [SerializeField] float rightLogShowInterval;

    [Header("Title Objects")]
    [SerializeField] GameObject titleText;
    [SerializeField] GameObject titleImage;

    [SerializeField] GameObject buttonText;
    [SerializeField] GameObject buttonBack;

    private string[] lines;





    void Start()
    {
        Init();
    }

    void Init()
    {
        videoPlayer = GetComponent<VideoPlayer>();
        videoPlayer.loopPointReached += OnVideoEnd;

        string leftfilePath = "Assets/Text/Tittle_Log_LeftAccess.txt";
        leftFileText = AssetDatabase.LoadAssetAtPath<TextAsset>(leftfilePath).text;

        string rightfilePath = "Assets/Text/Tittle_Log_RightLog.txt";
        rightFileText = AssetDatabase.LoadAssetAtPath<TextAsset>(rightfilePath).text;

        lines = rightFileText.Split('\n');

        InitActive();
    }

    void InitActive()
    {
        titleText.SetActive(false);
        titleImage.SetActive(false);
        buttonText.SetActive(false);
        buttonBack.SetActive(false);
    }





    /// <summary>
    /// ���� ����� ������ �� ��Ȱ��ȭ�ϴ� �Լ�
    /// </summary>
    /// <param name="vp"></param>
    void OnVideoEnd(VideoPlayer vp)
    {
        loadingVideo.SetActive(false);
        StartCoroutine(LeftLog());
    }
    




    /// <summary>
    /// ������� �α� ���
    /// </summary>
    /// <returns></returns>
    IEnumerator LeftLog()
    {
        int currentIndex = 0;
        leftLogField.text = "";

        while (currentIndex < leftFileText.Length)
        {
            char currentChar = leftFileText[currentIndex];
            leftLogField.text += currentChar;

            if (currentChar != '=')
            {
                yield return new WaitForSeconds(leftLogShowInterval);
            }
            
            currentIndex++;
        }

        StartCoroutine(RightLog());
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
            rightLogField.text += line;
            rightLogField.text += '\n';
            currentIndex++;

            scrollRect.verticalNormalizedPosition = 0.0f;

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
        yield return new WaitForSeconds(2f);

        titleText.SetActive(true);
        titleImage.SetActive(true);

        TextMeshProUGUI text = titleText.GetComponent<TextMeshProUGUI>();

        text.alpha = 0f;
        text.DOFade(1f, 0.1f).SetEase(Ease.InOutBounce);

        Image title = titleImage.GetComponent<Image>();

        title.color = new Color(1f, 1f, 1f, 0f);
        title.DOFade(1f, 0.1f).SetEase(Ease.InOutBounce);

        yield return new WaitForSeconds(2f);

        buttonText.SetActive(true);
        buttonBack.SetActive(true);

        App.instance.GetSoundManager().PlayBGM("BGM_TitleTheme");
    }
}
