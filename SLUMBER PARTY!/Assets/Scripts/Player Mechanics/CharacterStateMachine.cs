using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum StateID { Idle, Running, Jumping, Falling, Attacking, Stunned }

public class CharacterStateMachine : NetworkBehaviour
{
    private Dictionary<StateID, CharacterState> stateInstances = new();

    public NetworkVariable<StateID> CurrentStateID
        = new(StateID.Idle,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public NetworkVariable<int> CurrentAttackIndex
        = new(-1,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

    public void Initialize(StateID startingState, Dictionary<StateID, CharacterState> stateMap)
    {
        stateInstances = stateMap;
        ChangeState(startingState);
    }

    public void ChangeState(StateID newState, int moveIndex = -1)
    {
        if (!IsServer) return;

        // "Yeah it's not here boss"
        if (!stateInstances.ContainsKey(newState)) return;

        CurrentStateID.Value = newState;

        if (moveIndex != -1)
        {
            Debug.Log($"Somethin's fishy here! moveIndex: {moveIndex}");
            CurrentAttackIndex.Value = moveIndex;
        }

    }

    public void HandleStateChanged(StateID oldState, StateID newState)
    {
        if (stateInstances.ContainsKey(oldState))
            stateInstances[oldState].Exit();

        if (stateInstances.ContainsKey(newState))
            stateInstances[newState].Enter();

        stateInstances[CurrentStateID.Value].SetMove(CurrentAttackIndex.Value);   // only AttackState
    }
}
