using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    public static NetworkPlayer instance;

    public string playerName;

    public ulong ClientId;

    [SerializeField] private NetworkVariable<int> SelectedCharacter = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    void Awake()
    {
        if (instance == null)
        {
            instance = this;
            DontDestroyOnLoad(gameObject);
            ClientId = NetworkManager.LocalClientId;
            playerName = this.name;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void OnAwake()
    {
        DontDestroyOnLoad(gameObject);
    }

    public int GetCharacter() => SelectedCharacter.Value;
}
