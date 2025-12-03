using UnityEngine;

public class FallingState : PlayerState
{
    public FallingState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        Debug.Log("Current State: Falling");
    }

    // Called ONCE when exiting the state
    public override void Exit() { }

    // Called every frame
    public override void UpdateLogic() {
        if (player.isGrounded() && player._moveDirection.x == 0 && !player.fallingThrough)
        {
            stateMachine.ChangeState(player.idle);
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        // Air control while falling
        player.rb.linearVelocity = new Vector2(
            player._moveDirection.x * player.playerSpeed,
            player.rb.linearVelocity.y
        );
    }
}
