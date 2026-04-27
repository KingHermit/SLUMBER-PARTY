using SLUMBER_PARTY.LobbyUtils;
using UnityEngine;

public class LobbyListDisplay : MonoBehaviour
{
    [SerializeField] private LobbyItem[] lobbyCards;

    private void OnEnable()
    {
        if (TestLobby.Instance == null)
        {
            Debug.LogError("TestLobby.Instance is NULL. Is it in the scene?");
            return;
        }

        TestLobby.Instance.OnLobbiesListUpdated += RefreshAll;
        TestLobby.Instance.RefreshLobbyList();
    }

    private void OnDisable()
    {
        TestLobby.Instance.OnLobbiesListUpdated -= RefreshAll;
    }

    private void RefreshAll()
    {
        var lobbies = TestLobby.Instance.availableLobbies;

        for (int i = 0; i < lobbyCards.Length; i++)
        {
            if (i < lobbies.Count)
            {
                var lobbyName = lobbies[i].Name;
                var lobbyOccupancy = lobbies[i].MaxPlayers - lobbies[i].AvailableSlots;
                lobbyCards[i].UpdateDisplay(lobbyName, lobbyOccupancy);
            }
            else
            {
                lobbyCards[i].DisableDisplay();
            }
        }
    }
}
