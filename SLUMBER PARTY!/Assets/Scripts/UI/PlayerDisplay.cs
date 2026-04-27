using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using UnityEngine;

namespace SLUMBER_PARTY.LobbyUtils
{
    public class PlayerDisplay : MonoBehaviour
    {
        [SerializeField] private PlayerListItem[] playerCards;

        private void OnEnable()
        {
            if (TestLobby.Instance == null)
            {
                Debug.LogError("TestLobby.Instance is NULL. Is it in the scene?");
                return;
            }

            TestLobby.Instance.OnLobbyUpdated += RefreshAll;
        }

        private void OnDisable()
        {
            TestLobby.Instance.OnLobbyUpdated -= RefreshAll;
        }

        private void RefreshAll()
        {
            var lobby = TestLobby.Instance.GetJoinedLobby();

            var players = lobby.Players;

            for (int i = 0; i < playerCards.Length; i++)
            {
                if (i < players.Count)
                {
                    var playerName = players[i].Data["PlayerName"].Value;
                    playerCards[i].UpdateDisplay(playerName);
                }
                else
                {
                    playerCards[i].DisableDisplay();
                }
            }
        }
    }
}
