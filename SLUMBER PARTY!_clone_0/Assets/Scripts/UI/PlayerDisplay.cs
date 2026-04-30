using System;
using Unity.Services.Lobbies;
using Unity.Services.Lobbies.Models;
using Unity.Netcode;
using UnityEngine;
using Unity.Services.Multiplayer;

namespace SLUMBER_PARTY.LobbyUtils
{
    public class PlayerDisplay : MonoBehaviour
    {
        [SerializeField] private PlayerListItem[] playerCards;

        private void OnEnable()
        {
            if (TestSessionManager.Instance == null)
            {
                Debug.LogError("TestLobby.Instance is NULL. Is it in the scene?");
                return;
            }

            TestSessionManager.Instance.OnSessionUpdated += RefreshAll;
        }

        private void OnDestroy()
        {
            TestSessionManager.Instance.OnSessionUpdated -= RefreshAll;
        }

        private void RefreshAll()
        {
            var session = TestSessionManager.Instance.GetJoinedSession();

            var players = session.Players;

            for (int i = 0; i < playerCards.Length; i++)
            {
                if (i < players.Count)
                {
                    var playerName = players[i].GetPlayerName() ?? $"Player {i}";
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
