using UnityEngine;

public class JumpingState : PlayerState
{
    public JumpingState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        Debug.Log("Current state: Jumping");

        // jump ONE time
        player.rb.linearVelocity = new Vector2(
            player.rb.linearVelocity.x,
            player.jumpForce
        );
    }

    // Called ONCE when exiting the state
    public override void Exit() { }

    // Called every frame
    public override void UpdateLogic() {
        if (player.rb.linearVelocityY < 0)
        {
            stateMachine.ChangeState(player.falling);
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        // air horizontal control
        player.rb.linearVelocity = new Vector2(
            player._moveDirection.x * player.playerSpeed,
            player.rb.linearVelocity.y // <-- LEAVE Y ALONE!!!
        );
    }
}
