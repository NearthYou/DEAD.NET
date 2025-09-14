using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Controller
{
    bool isGameOver;
}

public class GameManager : Singleton<GameManager>
{
    Controller controller;
    MapCamera mapCamera;
    
    [Header("Tutorial Settings")]
    [SerializeField] bool skipTutorial = false;
    
    void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    void Update()
    {
        InputKey();
    }

    public void InputKey()
    {
    }

    public void UpdateAllState()
    {
    }

    public void SaveGame()
    {
    }

    public void PrevGameStart()
    {
        SceneLoader.instance.LoadScene(1);
    }

    public void NewGameStart()
    {
        SceneLoader.instance.LoadScene((int)ESceneType.Game);
        SceneLoader.instance.LoadSceneAdditive((int)ESceneType.Crafting);
        SceneLoader.instance.LoadSceneAdditive((int)ESceneType.UI);
        SceneLoader.instance.LoadSceneAdditive((int)ESceneType.Map);
        App.instance.GetSoundManager().StopBGM();
    }

    public void Settings()
    {
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }

    public bool ShouldSkipTutorial()
    {
        return skipTutorial;
    }
}