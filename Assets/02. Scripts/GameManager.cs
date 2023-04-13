using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public struct Controller
{
    bool isGameOver;
}

/// <summary>
/// �� ���� �Ѿ�� ���� �°� ��, ĳ����, �½�ũ ������Ʈ. -> �� �Ŵ������� ���.
/// UI, ��ȭ �ý��� ��Ʈ��.
/// ���� ���� �� ����, ���� ����, ���� ����.
/// �� ����.
/// </summary>
public class GameManager : Singleton<GameManager>
{
    Controller controller;

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
        // Ű �Է�
        if (Input.GetKeyDown(KeyCode.Space))
            Debug.Log("�����̽� ��");
        if (Input.GetKeyDown(KeyCode.Escape))
            QuitGame();

    }

    public void UpdateAllState()
    {
        // 1. ���� ����

        // 2. �� ������Ʈ

        // 3. ĳ���� ���� ������Ʈ

        // 4. �׽�ũ ������Ʈ
    }

    public void SaveGame()
    {
        // ���� ��� ������ ����
    }

    public void PrevGameStart()
    {
        // ���� ������ �ҷ�����

        // �� �̵�
        SceneLoader.instance.LoadScene(1);
    }

    public void NewGameStart()
    {
        // ������ �ʱ�ȭ

        // �� �̵�
        SceneLoader.instance.LoadScene(1);
    }

    public void Settings()
    {
        // ���� â
        Debug.Log("����");
    }

    public void QuitGame()
    {
#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
    }
}
