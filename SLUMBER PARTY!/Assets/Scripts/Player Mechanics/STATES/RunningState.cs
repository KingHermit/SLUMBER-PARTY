using UnityEngine;
using UnityEngine.EventSystems;

public class RunningState : PlayerState
{
    public RunningState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        Debug.Log("Current state: Running");
    }

    // Called ONCE when exiting the state
    public override void Exit() { }

    // Called every frame
    public override void UpdateLogic() {
        
        if (player._moveDirection.x == 0 && player.isGrounded())
        {
            stateMachine.ChangeState(player.idle);
            return;
        }

        if (player.jumpPressed)
        {
            player.jumpPressed = false;
            stateMachine.ChangeState(player.jumping);
            return;
        }

        if (!player.isGrounded())
        {
            stateMachine.ChangeState(player.falling);
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        player.rb.linearVelocity = new Vector2(
        player._moveDirection.x * player.playerSpeed,
        player.rb.linearVelocity.y
    );
    }
}
