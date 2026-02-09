using Unity.Netcode;
using UnityEngine;

public class StateSync : NetworkBehaviour
{
    private CharacterStateMachine _stateMachine;

    public NetworkVariable<StateID> CurrentStateID = new(StateID.Idle);
    public NetworkVariable<int> CurrentAttackIndex = new(-1);

    public void Initialize(CharacterStateMachine csm) => _stateMachine = csm;

    public override void OnNetworkSpawn()
    {
        Debug.Log("State changed!");
        CurrentStateID.OnValueChanged += SyncLocalStateServerRpc;
        CurrentAttackIndex.OnValueChanged += SyncLocalStateServerRpc;
    }

    [ServerRpc]
    private void SyncLocalStateServerRpc(StateID oldID, StateID newID) => ApplySyncClientRpc();
    [ServerRpc]
    private void SyncLocalStateServerRpc(int oldIdx, int newIdx) => ApplySyncClientRpc();

    [ClientRpc]
    private void ApplySyncClientRpc()
    {
        if (IsServer) return;
        _stateMachine.ChangeState(CurrentStateID.Value, CurrentAttackIndex.Value);
    }

    private void Update() => _stateMachine.currentState?.UpdateLogic();
    private void FixedUpdate()
    {
        if (IsServer || IsOwner) _stateMachine.currentState?.UpdatePhysics();
    }
}
