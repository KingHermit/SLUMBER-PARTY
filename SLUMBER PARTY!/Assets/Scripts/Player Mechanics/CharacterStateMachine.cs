using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem.LowLevel;

public enum StateID { Idle, Running, Jumping, Falling, Attacking, Stunned }

public class CharacterStateMachine : MonoBehaviour
{
    private Dictionary<StateID, CharacterState> stateInstances = new();
    public CharacterState currentState { get; private set; }

    // StateSync listens for event
    public System.Action<StateID, int?> OnStateTransitionTriggered;
    public void RegisterState(StateID id, CharacterState state) => stateInstances[id] = state;

    public void Initialize(StateID startingState, Dictionary<StateID, CharacterState> stateMap)
    {
        stateInstances = stateMap;
        ChangeState(startingState);
    }

    public void ChangeState(StateID newState, int moveIndex = -1)
    {
        // "Yeah it's not here boss"
        if (!stateInstances.ContainsKey(newState)) return;

        currentState?.Exit();
        currentState = stateInstances[newState];

        if (moveIndex != -1)
            currentState.SetMove(moveIndex);   // only AttackState

        currentState.Enter();

        // notify network
        OnStateTransitionTriggered?.Invoke(newState, moveIndex);
    }
}
