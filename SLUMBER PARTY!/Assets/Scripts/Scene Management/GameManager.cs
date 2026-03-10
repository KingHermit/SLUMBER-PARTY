using UnityEngine;
using Unity.Netcode;

public class GameManager : NetworkBehaviour
{
    private SceneLoader sceneLoader;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Awake()
    {
        DontDestroyOnLoad(gameObject);

        sceneLoader = GetComponentInChildren<SceneLoader>();
    }

    
}
