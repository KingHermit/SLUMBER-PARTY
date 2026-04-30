using System.Collections;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;

#region -- SCENE IDs --
public enum SceneID
{
    BOOTSTRAP = 0,
    MainMenu = 1,
    Lobby = 2,
    CharacterSelect = 3,
    TestingGrounds = 4,
    Stage = 5
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
        currentScene = SceneID.BOOTSTRAP;

        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);

        } else
        {
            Destroy(gameObject);
        }

        if (currentScene is SceneID.BOOTSTRAP)
        {
            ChangeGameScene(SceneID.MainMenu);
        }
    }
    #endregion -- Singleton --


    private void OnEnable()
    {
        NetworkManager.Singleton.OnClientDisconnectCallback += (id) =>
        {
            Debug.Log($"Client {id} disconnected. Current scene: {SceneManager.GetActiveScene().name}");
        };

        NetworkManager.Singleton.SceneManager.OnSceneEvent += (SceneEvent evt) =>
        {
            Debug.Log($"Scene Event: {evt.SceneEventType} for client {evt.ClientId}");
        };
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

    public void Quit()
    {
        Application.Quit();
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