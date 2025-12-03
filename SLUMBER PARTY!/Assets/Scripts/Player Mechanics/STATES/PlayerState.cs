using UnityEngine;

public abstract class PlayerState
{
    protected PlayerController player;
    protected PlayerStateMachine stateMachine;

    protected PlayerState(PlayerController player, PlayerStateMachine stateMachine)
    {
        this.player = player;
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
}
