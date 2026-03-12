using UnityEngine;

public class UI_SceneButtons : MonoBehaviour {

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

    public void LoadTestingGrounds()
    {
        GameManager.instance.ChangeGameScene(SceneID.TestingGrounds);
    }
}

