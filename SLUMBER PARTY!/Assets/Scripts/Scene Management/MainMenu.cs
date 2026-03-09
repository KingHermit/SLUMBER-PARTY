using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenu : MonoBehaviour
{
    public void PlayGame()
    {
        SceneManager.LoadScene("Testing Grounds");
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
