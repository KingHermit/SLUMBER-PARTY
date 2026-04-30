using SLUMBER_PARTY.LobbyUtils;
using UnityEngine;

public class SessionListDisplay : MonoBehaviour
{
    [SerializeField] private LobbyItem[] lobbyCards;

    private void OnEnable()
    {
        if (TestSessionManager.Instance == null)
        {
            Debug.LogError("TestSession.Instance is NULL. Is it in the scene?");
            return;
        }

        TestSessionManager.Instance.OnSessionsListUpdated += RefreshAll;
        TestSessionManager.Instance.RefreshSessionList();


    }

    private void OnDestroy()
    {
        TestSessionManager.Instance.OnSessionsListUpdated -= RefreshAll;
    }

    private void RefreshAll()
    {
        var sessions = TestSessionManager.Instance.availableSessions;

        if (sessions.Count == 0) return;

        for (int i = 0; i < lobbyCards.Length - 1; i++)
        {
            var session = sessions[i];

            // ISession uses Name and MaxPlayers, but 'AvailableSlots' 
            // is usually calculated as MaxPlayers - Players.Count
            var sessionName = session.Name;
            var max = session.MaxPlayers;
            var occupancy = max - session.AvailableSlots;

            lobbyCards[i].UpdateDisplay(sessionName, occupancy);
        }
    }
}
