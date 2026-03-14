using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using NUnit.Framework;
using System.Collections;
using System.Collections.Generic;
using Unity.Services.Matchmaker.Models;

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

    [Header("-- PLAYERS --")]
    public Transform networkPlayersHolder;
    [SerializeField] private NetworkObject playerPrefab;
    [SerializeField] private List<NetworkPlayer> playerCharacterList; // figure out how to store player selected character

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

    public override void OnNetworkSpawn()
    {
        playerCharacterList = new List<NetworkPlayer>();

        if (IsClient)
        {

        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback += HandleClientDisconnected;
        }
    }

    public override void OnNetworkDespawn()
    {
        if (IsClient)
        {

        }

        if (IsServer)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientConnected;
            NetworkManager.Singleton.OnClientConnectedCallback -= HandleClientDisconnected;
        }
    }

    private void HandleClientConnected(ulong clientId)
    {
        NetworkObject newPlayer = Instantiate(playerPrefab, networkPlayersHolder);

        if (newPlayer != null)
        {
            playerCharacterList.Add(newPlayer.GetComponent<NetworkPlayer>());
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < playerCharacterList.Count; i++)
        {
            if (playerCharacterList[i].ClientId == clientId)
            {
                //playerCharacterList.RemoveAt(i);
                Destroy(playerCharacterList[i]);
                break;
            }
        }
    }

    #region -- SCENE LOADING --

    public void ChangeGameScene(SceneID newScene)
    {
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

    #endregion -- SCENE LOADING --
}
