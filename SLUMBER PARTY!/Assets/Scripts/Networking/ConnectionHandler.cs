using UnityEngine;
using Unity.Netcode;
using System.Collections.Generic;
using SLUMBER_PARTY.LobbyUtils;
using UnityEngine.SceneManagement;
using System;

public class ConnectionHandler : NetworkBehaviour
{
    public NetworkObject playerPrefab;
    HashSet<ulong> connected = new();
    //private HashSet<ulong> pendingPlayers = new();

    private void Awake()
    {
        Debug.Log("Connection Handler awake!");
        DontDestroyOnLoad(gameObject);
    }

    private void OnEnable()
    {
        if (NetworkManager.Singleton == null)
        {
            Debug.LogError("NetworkManager is NULL");
            return;
        }

        //NetworkManager.Singleton.SceneManager.OnLoadEventCompleted += OnSceneLoaded;
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;

    }

    //private void OnSceneLoaded(string sceneName, LoadSceneMode loadSceneMode, List<ulong> clientsCompleted, List<ulong> clientsTimedOut)
    //{
    //    foreach (var clientId in pendingPlayers)
    //    {
    //        Debug.Log("Attempting spawn");
    //        var player = Instantiate(playerPrefab);
    //        player.GetComponent<NetworkObject>().SpawnAsPlayerObject(clientId);
    //        Debug.Log("Spawn call executed");
    //    }

    //    pendingPlayers.Clear();
    //}

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.OnClientConnectedCallback -= OnClientConnected;
            //NetworkManager.SceneManager.OnLoadEventCompleted -= OnSceneLoaded;
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        Debug.Log("OH HELLO THERE");
        //pendingPlayers.Add(clientId);

        if (!NetworkManager.Singleton.IsServer) return;

        var player = Instantiate(playerPrefab);
        player.GetComponent<NetworkObject>()
              .SpawnAsPlayerObject(clientId);

        connected.Add(clientId);

        CheckAllPlayersConnected();
    }

    private void CheckAllPlayersConnected()
    {
        //if (connected.Count == 2)
        //{
        //    GameManager.instance.ChangeGameScene(SceneID.CharacterSelect);
        //}

        GameManager.instance.ChangeGameScene(SceneID.CharacterSelect);
    }
}