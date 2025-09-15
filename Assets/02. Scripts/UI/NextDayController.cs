using System.Collections;
using Cinemachine;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using Febucci.UI;

public class NextDayController : MonoBehaviour
{
    [SerializeField] Image blackPanel;
    [SerializeField] GameObject dayCountPrefab;
    GameObject dayCount;
    [SerializeField] MapIcon mapIcon;

    CinemachineVirtualCamera mapCamera;
    CameraShake normalCamera;
    CinemachineFramingTransposer transposer;

    [HideInInspector] public bool isOver = false;
    [HideInInspector] public bool isHit = false;

    void Start()
    {
        mapCamera = GameObject.FindGameObjectWithTag("MapCamera").GetComponent<CinemachineVirtualCamera>();
        normalCamera = GameObject.FindGameObjectWithTag("MainCamera").GetComponent<CameraShake>();

        transposer = mapCamera.GetCinemachineComponent<CinemachineFramingTransposer>();
        transposer.m_CameraDistance = 5f;

        isOver = false;
    }

    public void InitBlackPanel()
    {
        if (isHit == true)
        {
            normalCamera.Shake();
        }
        else
            UIManager.instance.GetUpperController().UpdateDurabillity();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(blackPanel.DOFade(0f, 1f).SetEase(Ease.Linear))
            .OnComplete(() => blackPanel.gameObject.SetActive(false));

        Destroy(dayCount);

        isHit = false;

        UIManager.instance.PopCurrUI();

        UIManager.instance.GetNoteController().isNewDay = true;
    }


    /// <summary>
    /// 다음 날로 넘어갈 때 호출되는 이벤트 --> blackPanel 깜빡거림
    /// </summary>
    public void NextDayEvent()
    {
        UIManager.instance.AddCurrUIName("UI_LOADING");

        blackPanel.gameObject.SetActive(true);

        App.instance.GetSoundManager().StopBGM();

        Sequence sequence = DOTween.Sequence();
        sequence.Append(blackPanel.DOFade(1f, 0.5f)).SetEase(Ease.InQuint)
            .OnComplete(() =>
            {
                StartCoroutine(NextDayEventCallBack(() =>
                {
                    if (isOver == true)
                        StartCoroutine(ShowGameOver());
                    else
                        StartCoroutine(ShowNextDate());
                }));
            });
    }

    /// <summary>
    /// NextDayEvent 콜백함수
    /// </summary>
    IEnumerator NextDayEventCallBack(System.Action callback)
    {
        UIManager.instance.GetAlertController().InitAlert();

        App.instance.GetMapManager().SetMapCameraPriority(false);
        transposer.m_CameraDistance = 5f;

        yield return StartCoroutine(NextDayCoroutineWrapper());

        yield return new WaitForSeconds(1f);

        SetResourcesResultPage();

        callback?.Invoke();
    }

    IEnumerator NextDayCoroutineWrapper()
    {
        // UniTask를 시작하고 완료될 때까지 대기
        var task = App.instance.GetMapManager().NextDayCoroutineAsync();
        while (!task.GetAwaiter().IsCompleted)
        {
            yield return null;
        }
    }

    IEnumerator ShowNextDate()
    {
        UIManager.instance.GetNoteController().SetNextDay();
        UIManager.instance.GetCraftingUiController().EquipItemDayEvent();

        int today = UIManager.instance.GetNoteController().dayCount;

        string text = "<color=white>Day " + "{vertexp}" + today.ToString() + "{/vertexp}</color>";

        if (isHit == true)
            text = "<color=red><shake a=0.1>" + "Day " + "{vertexp}" + today.ToString() + "{/vertexp}</shake></color>";

        CreateDayCountTxt(text);
        mapIcon.SetIconImage();

        yield return new WaitForSeconds(2f);

        InitBlackPanel();

        App.instance.GetSoundManager().PlayBGM("BGM_InGameTheme");
    }


    IEnumerator ShowGameOver()
    {
        string text = "<color=red><shake a=0.1>GAME OVER</shake></color>";
        CreateDayCountTxt(text);

        yield return new WaitForSeconds(2f);

        GameManager.instance.QuitGame();
    }

    void CreateDayCountTxt(string _text)
    {
        dayCount = Instantiate(dayCountPrefab, blackPanel.transform);
        TextMeshProUGUI text = dayCount.GetComponent<TextMeshProUGUI>();
        text.text = _text;
    }


    void SetResourcesResultPage()
    {
        var resources = App.instance.GetMapManager().resourceManager.GetLastResources();

        for (int i = 0; i < resources.Count; i++)
        {
            string tileName = App.instance.GetMapManager().mapController
                .Player.TileController.GetComponent<TileBase>().TileData.English;

            int randomNumber = Random.Range(1, 6);

            string nodeName = resources[i].ItemBase.data.Code + "_" + tileName + randomNumber.ToString();

            if (resources[i].ItemBase.data.Code == "ITEM_NETWORKCHIP") 
                UIManager.instance.GetPageController().SetResultPage(nodeName, false);
            else
                UIManager.instance.GetPageController().SetResultPage(nodeName, true);
        }
    }

    /// <summary>
    /// 맵->기지 카메라 이동
    /// </summary>
    public void GoToLab()
    {
        blackPanel.gameObject.SetActive(true);

        App.instance.GetSoundManager().StopBGM();
        App.instance.GetSoundManager().PlaySFX("SFX_SceneChange_MapToBase");

        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(blackPanel.DOFade(1f, 0.5f))
            .Join(DOTween.To(() => transposer.m_CameraDistance, x => transposer.m_CameraDistance = x, 5f, 0.5f))
            .AppendCallback(() =>
            {
                App.instance.GetMapManager().SetMapCameraPriority(false);
                App.instance.GetSoundManager().PlayBGM("BGM_InGameTheme");
                App.instance.GetMapUiController().FalseTileInfo();
            })
            .Append(blackPanel.DOFade(0f, 1f))
            .OnComplete(() => blackPanel.gameObject.SetActive(false));
    }

    /// <summary>
    /// 기지->맵 카메라 이동
    /// </summary>
    public void GoToMap()
    {
        blackPanel.gameObject.SetActive(true);

        App.instance.GetSoundManager().StopBGM();

        Sequence sequence = DOTween.Sequence();
        sequence
            .Append(blackPanel.DOFade(1f, 0.5f))
            .AppendCallback(() =>
            {
                App.instance.GetMapUiController().FalseTileInfo();
                App.instance.GetSoundManager().PlaySFX("SFX_SceneChange_BaseToMap");

                App.instance.GetMapManager().SetMapCameraPriority(true);
                App.instance.GetMapManager().CheckLandformPlayMusic();
            })
            .Append(DOTween.To(() => transposer.m_CameraDistance, x => transposer.m_CameraDistance = x, 10f, 0.5f))
            .Join(blackPanel.DOFade(0f, 0.5f))
            .OnComplete(() =>
                {
                    blackPanel.gameObject.SetActive(false);
                    App.instance.GetMapManager().GetCameraCenterTile();
                    App.instance.GetMapManager().InvocationExplorers();
                }
            );
    }
}