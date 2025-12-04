using UnityEngine;

public class IdleState : PlayerState
{
    public IdleState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        Debug.Log("Current state: Idle");
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        
    }

    // Called every frame
    public override void UpdateLogic() {
        // Run Transition
        if (Mathf.Abs(player._moveDirection.x) != 0 && player.isGrounded())
        {
            stateMachine.ChangeState(player.running);
            return;
        }

        // Jump Transition
        if (player.jumpPressed)
        {
            player.jumpPressed = false;
            stateMachine.ChangeState(player.jumping);
            return;
        }

        // Falling transition
        if (!player.isGrounded())
        {
            stateMachine.ChangeState(player.falling);
            return;
        }

        if (player.isAttacking) // erm
        {
            player.isAttacking = false;
            stateMachine.ChangeState(player.attacking);
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        player.rb.linearVelocity = new Vector2(
        player._moveDirection.x,
        player.rb.linearVelocity.y
    );
    }
}
