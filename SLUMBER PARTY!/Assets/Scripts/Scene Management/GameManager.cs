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

    //[Header("MAIN STAGE")]

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
        //playerCharacterList = new List<NetworkPlayer>();

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
        //RowData newPlayer = new RowData();
        //PlayerCell info = new PlayerCell();
        

        try
        {
            //info.playerName = "Beebus";
            //info.currentHealth = 0;
            //info.isReady = false;

            //newPlayer.columns.Add(info);

            //NetworkObject newPlayerNetworkObject = Instantiate(playerPrefab);
            //newPlayerNetworkObject.TrySetParent(networkPlayersHolder);
            //NetworkPlayer playerInfo = newPlayerNetworkObject.GetComponent<NetworkPlayer>();
            //playerInfo.ClientId = clientId;

            //playerCharacterList.Add(playerInfo);

            //Debug.Log($"Player: {playerInfo.ClientId}  |  " +
            //    $"Character: {characterDatabase.GetCharacterById(playerInfo.GetCharacter())}");
        }
        catch (NullReferenceException e)
        {
            Debug.Log(e.Message);
        }
    }

    private void HandleClientDisconnected(ulong clientId)
    {
        for (int i = 0; i < playerCharacterList.Count; i++)
        {
            //if (playerCharacterList[i].ClientId == clientId)
            //{
            //    //playerCharacterList.RemoveAt(i);
            //    Destroy(playerCharacterList[i]);
            //    break;
            //}
        }
    }

    #region -- SCENE LOADING --

    public void ChangeGameScene(SceneID newScene)
    {
        //switch (newScene)
        //{
        //    case SceneID.MainMenu:
        //        Debug.Log("To the main menu!");
        //        break;
        //    case SceneID.Lobby:
        //        Debug.Log("To the lobby!");
        //        break;
        //    case SceneID.CharacterSelect:
        //        Debug.Log("Who up choosing they character?");
        //        break;
        //    case SceneID.Stage:
        //        Debug.Log("The actual main stage isn't ready yet sorry guys");
        //        break;
        //}
        
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

        else if (NetworkManager.Singleton.IsServer)
        {
            LoadNetworkScene(scene);
            return;
        } else
        {
            RequestSceneLoadServerRpc(scene);
            return;
        }
    }

    private void LoadNetworkScene(SceneID scene)
    {
        isLoading = true;

        NetworkManager.Singleton.SceneManager.LoadScene(scene.ToString(), LoadSceneMode.Single);
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