using UnityEngine;
using SLUMBER_PARTY.LobbyUtils;
using UnityEngine.UI;
using Unity.Netcode;


public class SessionFunks : MonoBehaviour
{
    [SerializeField] private Button m_startButton;

    private void Awake()
    {
        m_startButton = GetComponentInChildren<Button>();
        TestSessionManager.Instance.OnSessionUpdated += updateButton;

        m_startButton.onClick.AddListener(OnStartGamePressed);
    }

    private void updateButton()
    {
        if (!TestSessionManager.Instance.IsSessionHost())
            m_startButton.interactable = false;
        else
            m_startButton.interactable = true;
    }

    public void OnStartGamePressed()
    {
        Debug.Log("MOVE MOVE MOVE!!!!!");
        TestSessionManager.Instance.StartGame();
        //GameManager.instance.ChangeGameScene(SceneID.CharacterSelect); // doesnt wait for the client to start D:
    }

    public void LeaveLobby()
    {
        TestSessionManager.Instance.Leave();
    }
}
