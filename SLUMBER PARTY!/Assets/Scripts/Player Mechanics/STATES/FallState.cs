using UnityEngine;

public class FallingState : PlayerState
{
    public FallingState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        player._animator.SetBool("isFalling", true);
        Debug.Log("Current State: Falling");
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        player._animator.SetBool("isFalling", false);
    }

    // Called every frame
    public override void UpdateLogic() {
        if (player.isGrounded() && player._moveDirection.x == 0 && !player.fallingThrough)
        {
            stateMachine.ChangeState(player.idle);
            return;
        }

        if (player.isAttacking)
        {
            player.isAttacking = false;
            stateMachine.ChangeState(player.attacking);
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        // Air control while falling
        player.rb.linearVelocity = new Vector2(
            player._moveDirection.x * player.playerSpeed,
            Mathf.Max(player.rb.linearVelocity.y, -player.maxFallSpeed)
        );
    }
}
