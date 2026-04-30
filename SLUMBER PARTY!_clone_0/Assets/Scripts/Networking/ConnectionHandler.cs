using SLUMBER_PARTY.LobbyUtils;
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.SceneManagement;
using static Unity.Netcode.NetworkManager;

public class ConnectionHandler : NetworkBehaviour
{
    public NetworkObject playerPrefab;
    HashSet<ulong> connected = new();
    //private HashSet<ulong> pendingPlayers = new();

    void Awake()
    {
        Debug.Log("Connection Handler awake!");
        DontDestroyOnLoad(gameObject);

        if (NetworkManager.Singleton == null) { return; }

        if (IsServer)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback = ApprovalCheck;
            NetworkManager.Singleton.OnTransportFailure += Notify;
        }
            
    }

    private void ApprovalCheck(NetworkManager.ConnectionApprovalRequest request, NetworkManager.ConnectionApprovalResponse response)
    {
        Debug.Log("I hear u and I see u");

        // 1) chekc logic (custom later)
        bool isApproved = true;

        // 2) respond to request
        response.Approved = isApproved;
        response.CreatePlayerObject = true;

        // 3) If denying connection
        if (!isApproved)
        {
            response.Reason = "Server is full or version mismatch.";
        }

        // 4) Must set to false if you need more time for an async check
        response.Pending = false;
    }

    private void OnDisable()
    {
        if (NetworkManager.Singleton != null)
        {
            NetworkManager.Singleton.ConnectionApprovalCallback -= ApprovalCheck;
        }
    }

    private void Notify()
    {
        Debug.Log("Shit's having CONNECTION PROBLEMS");
    }

    private void OnClientConnected(ulong clientId)
    {
        //if (!NetworkManager.Singleton.IsServer) return;

        //var player = Instantiate(playerPrefab);
        //player.GetComponent<NetworkObject>()
        //      .SpawnAsPlayerObject(clientId);

        //connected.Add(clientId);
        // new issue: on client instance, host prefab doesn't spawn. also, scene change doesnt work.

        // CheckAllPlayersConnected();
    }

    private void CheckAllPlayersConnected()
    {
        //if (connected.Count >= 1)
        //{
        //    GameManager.instance.ChangeGameScene(SceneID.CharacterSelect);
        //}

        //GameManager.instance.ChangeGameScene(SceneID.CharacterSelect);
    }
}