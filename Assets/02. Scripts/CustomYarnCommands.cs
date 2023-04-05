using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Yarn.Unity;
using DG.Tweening;

[System.Serializable]
public class Images
{
    public string name;
    public Sprite sprite;
}

public class CustomYarnCommands : Singleton<CustomYarnCommands>
{
    [SerializeField] SpriteRenderer[] bgRenderers;
    [SerializeField] Images[] img = null;
    
    public LineView lineView;
    public DialogueRunner dialogueRunner;
    public bool imgOrder=false;

    Dictionary<string, Sprite> dic_IMG;
    Sequence sequence;

    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
        dic_IMG = new Dictionary<string, Sprite>();
        dialogueRunner.AddCommandHandler<string>("change_img", ChageImages);
        dialogueRunner.AddCommandHandler<string>("play_bgm", PlayBGM);
        dialogueRunner.AddCommandHandler<string>("play_sfx", PlaySFX);
        dialogueRunner.AddCommandHandler<string>("stop_bgm", StopBGM);
    }

    void Start()
    {
        foreach (Images images in img)
        {
            dic_IMG.Add(images.name, images.sprite);
        }
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Space))
        {
            lineView.UserRequestedViewAdvancement();
        }

        if (dialogueRunner.runSelectedOptionAsLine)
        {
            Debug.Log("���õ�");
        }
            
    }

    private void ChageImages(string imgName)
    {
        var target = imgOrder ? bgRenderers[0] : bgRenderers[1];
        var prev = imgOrder ? bgRenderers[1] : bgRenderers[0];

        imgOrder = !imgOrder;

        if (target == null || prev == null)
        {
            Debug.Log("Can't find the target!");
        }

        prev.sortingOrder -= 1;
        target.sortingOrder += 1;

        target.sprite = dic_IMG[imgName];

        sequence = DOTween.Sequence()
            .OnStart(() =>
            {
                target.color = new Color32(255, 255, 255, 0);
                target.gameObject.SetActive(true);
            })
            .Append(target.DOFade(1, 1f))
            .OnComplete(() =>
            {
                prev.gameObject.SetActive(false);
            });
    }

    private void PlayBGM(string bgmName)
    {
        SoundManager.instance.PlayBGM(bgmName);
    }

    private void PlaySFX(string sfxName)
    {
        SoundManager.instance.PlaySFX(sfxName);
    }

    private void StopBGM(string soundName)
    {
        SoundManager.instance.StopBGM();
    }
}