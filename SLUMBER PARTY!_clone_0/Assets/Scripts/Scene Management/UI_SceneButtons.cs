using UnityEngine;

namespace SLUMBER_PARTY.LobbyUtils
{
    public class UI_SceneButtons : MonoBehaviour
    {
        public void QuitGame()
        {
            #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
            #else
                Application.Quit();
            #endif
        }

        public void LoadMainMenu()
        {
            GameManager.instance.ChangeGameScene(SceneID.MainMenu);
        }

        public void LoadLobby()
        {
            GameManager.instance.ChangeGameScene(SceneID.Lobby);
        }

        public void LoadCharacterSelect()
        {
            GameManager.instance.ChangeGameScene(SceneID.CharacterSelect);
        }

        public void LoadStage()
        {
            GameManager.instance.ChangeGameScene(SceneID.Stage);
        }
    }
}

