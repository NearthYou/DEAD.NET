using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

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

    private void Awake()
    {
        DontDestroyOnLoad(this.gameObject);
    }

    public void UpdateAllState()
    {
        // ���� ���� �� ��� ������ ����

        // 1. ���� ������Ʈ

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
        SceneManager.LoadScene("02. GameScene");
    }

    public void NewGameStart()
    {
        // ������ �ʱ�ȭ

        // �� �̵�
        SceneManager.LoadScene("02. GameScene");
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
