using UnityEngine;

public class PlayerStateMachine : MonoBehaviour
{
    public PlayerState currentState { get; private set; }


    public void Initialize(PlayerState startingState)
    {
        currentState = startingState;
        startingState.Enter();
    }

    public void ChangeState(PlayerState newState)
    {
        currentState.Exit();
        currentState = newState;
        newState.Enter();
    }

    public void ChangeState(PlayerState newState, MoveData move)
    {
        currentState?.Exit();
        newState.SetMove(move);   // only AttackState will care about this
        currentState = newState;
        currentState.Enter();
    }
}
