using UnityEngine;
using SLUMBER_PARTY.LobbyUtils;
public class StartMatch : MonoBehaviour
{
    public void OnStartGamePressed()
    {
        TestLobby.Instance.StartGame();
    }
}
