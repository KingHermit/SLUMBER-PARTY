using Unity.Netcode;
using UnityEngine;
using System.Collections.Generic;

public class PlayerHealthUIManager : MonoBehaviour
{
    private Dictionary<ulong, PlayerHealthUI> playerUIMap = new();
    public GameObject m_healthItem;
    public Transform healthHolder;

    private void Start()
    {
        NetworkManager.Singleton.OnClientConnectedCallback += OnClientConnected;
        NetworkManager.Singleton.OnClientDisconnectCallback += OnClientDisconnected;

        // Hook existing players (important if joining mid-game)
        foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
        {
            SetupPlayer(client);
        }
    }

    private void OnClientConnected(ulong clientId)
    {
        var client = NetworkManager.Singleton.ConnectedClients[clientId];
        SetupPlayer(client);
    }

    private void SetupPlayer(NetworkClient client)
    {
        var playerObj = client.PlayerObject.GetComponent<PlayerController>();
        if (playerObj == null) return;

        // Create UI element however you want
        var ui = CreateUIForPlayer(client.ClientId);

        playerUIMap[client.ClientId] = ui;

        // Initial value
        ui.UpdateHealth(playerObj.Health.Value);

        // Subscribe to changes
        playerObj.Health.OnValueChanged += (oldVal, newVal) =>
        {
            ui.UpdateHealth(newVal);
        };
    }

    private void OnClientDisconnected(ulong clientId)
    {
        if (playerUIMap.TryGetValue(clientId, out var ui))
        {
            Destroy(ui.gameObject);
            playerUIMap.Remove(clientId);
        }
    }

    private PlayerHealthUI CreateUIForPlayer(ulong clientId)
    {
        var obj = Instantiate(m_healthItem, healthHolder);
        return obj.GetComponent<PlayerHealthUI>();
    }
}