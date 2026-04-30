using TMPro;
using UnityEngine;
using WebSocketSharp;

namespace SLUMBER_PARTY.LobbyUtils
{
    public class LobbyItem : MonoBehaviour
    {
        [SerializeField] private GameObject visuals;
        [SerializeField] private TMP_Text m_lobbyNameText;
        [SerializeField] private TMP_Text m_occupancyText;

        public void UpdateDisplay(string lobbyName, int occupancy)
        {
            if (!lobbyName.IsNullOrEmpty())
            {
                m_lobbyNameText.text = lobbyName.ToString();
                m_occupancyText.text = occupancy.ToString();
                visuals.SetActive(true);
            }
            else
            {
                DisableDisplay();
            }
        }

        public void DisableDisplay()
        {
            visuals.SetActive(false);
        }
    }
}

