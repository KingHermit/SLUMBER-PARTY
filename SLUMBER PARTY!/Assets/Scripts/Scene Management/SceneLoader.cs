using UnityEngine;
using Unity.Netcode;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class SceneLoader : NetworkBehaviour
{
    private Scene currentScene;
    public Button m_startGame_btn;
    public Button m_startMatch_btn;


    private void Awake()
    {
        DontDestroyOnLoad(gameObject);
        currentScene = SceneManager.GetActiveScene();
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        m_startGame_btn.onClick.AddListener(delegate { LoadSceneAllPlayers(2); });
        m_startMatch_btn.onClick.AddListener(delegate { LoadSceneAllPlayers(3); });
    }

    // Update is called once per frame
    void Update()
    {
        switch (currentScene.name)
        {
            case "Main Menu":
                Debug.Log($"We're in the main menu!");
                return;
            case "Lobby":
                Debug.Log($"We're in lobby creation! Next button: {m_startGame_btn.name}");
                return;
            case "Character Select":
                Debug.Log($"Who up selecting they character? Next button: {m_startMatch_btn.name}");
                return;
            case "Main Stage":
                Debug.Log($"PILLOW FIGHT!!!");
                return;
        }
    }

    private void LoadSceneAllPlayers(int sceneNumber)
    {
        NetworkManager.SceneManager.LoadScene(SceneUtility.GetScenePathByBuildIndex(sceneNumber), LoadSceneMode.Single);
        currentScene = SceneManager.GetActiveScene();
    }

    public string GetCurrentScene()
    {
        return currentScene.name;
    }
}
