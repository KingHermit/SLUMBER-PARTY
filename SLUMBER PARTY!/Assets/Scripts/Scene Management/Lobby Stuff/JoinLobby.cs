using TMPro;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

namespace SLUMBER_PARTY.LobbyUtils
{

    public class JoinLobby : MonoBehaviour
    {
        [SerializeField] private TMP_InputField m_inputField;
        private Button joinLobbyButton;

        public void Awake()
        {
            m_inputField = GetComponentInChildren<TMP_InputField>();
            joinLobbyButton = GetComponentInChildren<Button>();

            m_inputField.onEndEdit.AddListener(value =>
            {
                if (Input.GetKeyDown(KeyCode.Return) && !string.IsNullOrEmpty(value))
                {
                    TestLobby.Instance.JoinLobbyByCode(value);
                }
            });

            m_inputField.onValueChanged.AddListener(value =>
            {
                joinLobbyButton.interactable = !string.IsNullOrEmpty(value) && TestLobby.Instance.GetJoinedLobby() == null;
            });
        }
    }
}


