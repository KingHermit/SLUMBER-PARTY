using UnityEngine;

public class CharacterStateMachine : MonoBehaviour
{
    public CharacterState currentState { get; private set; }


    public void Initialize(CharacterState startingState)
    {
        currentState = startingState;
        startingState.Enter();
    }

    public void ChangeState(CharacterState newState)
    {
        currentState.Exit();
        currentState = newState;
        newState.Enter();
    }

    public void ChangeState(CharacterState newState, MoveData move)
    {
        currentState?.Exit();
        newState.SetMove(move);   // only AttackState
        currentState = newState;
        currentState.Enter();
    }
}
