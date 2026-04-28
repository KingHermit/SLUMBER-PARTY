using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Entities;
using Unity.Netcode;
using UnityEditor;
using Unity.Services.Matchmaker.Models;
using UnityEngine;
using UnityEngine.SceneManagement;

#region -- SCENE IDs --
public enum SceneID
{
    MainMenu = 0,
    Lobby = 1,
    CharacterSelect = 2,
    TestingGrounds = 3,
    Stage = 4
}
#endregion -- SCENE IDs --

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    [SerializeField] private bool isLoading;

    public SceneID currentScene;

    [Header("-- PLAYERS --")]

    public List<NetworkPlayer> playerCharacterList = new List<NetworkPlayer>(4);

    #region -- Singleton --
    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

        } else
        {
            Destroy(gameObject);
        }
    }
    #endregion -- Singleton --


    private void OnEnable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
            NetworkManager.Singleton.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
    }

    private void OnSceneLoaded(string sceneName, LoadSceneMode mode,
    List<ulong> completed, List<ulong> timedOut)
    {
        isLoading = false;
    }

    #region -- SCENE LOADING --

    public void ChangeGameScene(SceneID newScene)
    {
        LoadScene(newScene);
        currentScene = newScene;
    }


    private void LoadScene(SceneID scene)
    {
        if (isLoading) return;

        var nm = NetworkManager.Singleton;

        if (nm == null || !nm.IsListening) // OFFLINE
        {
            StartCoroutine(LoadOffline(scene));
            return;
        }

        if (nm.IsServer) // ONLINE
        {
            LoadNetworkScene(scene);
        }
        
        // CLIENTS DON'T DO CRAP
    }

    private void LoadNetworkScene(SceneID scene)
    {
        isLoading = true;

        NetworkManager.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);

        isLoading = false;
    }

    IEnumerator LoadOffline(SceneID scene)
    {
        isLoading = true;

        AsyncOperation op = SceneManager.LoadSceneAsync(scene.ToString());

        while (!op.isDone)
        {
            yield return null;
        }

        isLoading = false;
    }

    #endregion -- SCENE LOADING --
}