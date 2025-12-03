using UnityEngine;

public class AttackState : PlayerState
{
    public AttackState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() { }

    // Called ONCE when exiting the state
    public override void Exit() { }

    // Called every frame
    public override void UpdateLogic() { }

    // Called every physics frame
    public override void UpdatePhysics() { }
}
