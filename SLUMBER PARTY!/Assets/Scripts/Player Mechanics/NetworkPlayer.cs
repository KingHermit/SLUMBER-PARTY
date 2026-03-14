using UnityEngine;
using Unity.Netcode;

public class NetworkPlayer : NetworkBehaviour
{
    public ulong ClientId;

    [SerializeField] private NetworkVariable<int> SelectedCharacter = new NetworkVariable<int>(
        -1,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Server
        );

    private void OnAwake()
    {
        DontDestroyOnLoad(gameObject);
    }
}
