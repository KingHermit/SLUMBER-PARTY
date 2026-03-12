using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;

#region -- SCENE IDs --
public enum SceneID
{
    MainMenu = 0,
    Lobby = 1,
    CharacterSelect = 2,
    TestingGrounds = 3,
    MainStage = 4
}
#endregion -- SCENE IDs --

public class GameManager : NetworkBehaviour
{
    public static GameManager instance;
    bool isLoading;

    [SerializeField] private SceneID currentScene;

    [Header("MAIN STAGE")]
    [SerializeField] private List<Transform> playerSpawnAreas = new List<Transform>(4);

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


    public void ChangeGameScene(SceneID newScene)
    {
        //var newScene = currentScene + 1;

        switch (newScene)
        {
            case SceneID.MainMenu:
                Debug.Log("To the main menu!");
                break;
            case SceneID.Lobby:
                Debug.Log("To the lobby!");
                break;
            case SceneID.CharacterSelect:
                Debug.Log("Who up choosing they character?");
                break;
            case SceneID.TestingGrounds:
                Debug.Log("The actual main stage isn't ready yet sorry guys");
                break;
        }
        
        LoadScene(newScene);
        currentScene = newScene;
    }


    private void LoadScene(SceneID scene)
    {
        if (isLoading) return;
        

        if (NetworkManager.Singleton == null) // Offline 
        {
            StartCoroutine(LoadOffline(scene));
            return;
        }

        if (NetworkManager.Singleton.IsServer)
        {
            LoadNetworkScene(scene);
        } else
        {
            RequestSceneLoadServerRpc(scene);
        }
    }

    private void LoadNetworkScene(SceneID scene)
    {
        isLoading = true;

        NetworkManager.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
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

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    void RequestSceneLoadServerRpc(SceneID scene)
    {
        LoadNetworkScene(scene);
    }
}
