using UnityEngine;

public abstract class CharacterState
{
    protected CharacterController controller;
    protected CharacterStateMachine stateMachine;
    protected MoveData move;

    protected CharacterState(CharacterController controller, CharacterStateMachine stateMachine)
    {
        this.controller = controller;
        this.stateMachine = stateMachine;
    }

    // Called ONCE when entering the state
    public abstract void Enter();

    // Called ONCE when exiting the state
    public abstract void Exit();

    // Called every frame
    public abstract void UpdateLogic();

    // Called every physics frame
    public abstract void UpdatePhysics();

    // Only for Attacking State
    public virtual void SetMove(MoveData m) { }
}
