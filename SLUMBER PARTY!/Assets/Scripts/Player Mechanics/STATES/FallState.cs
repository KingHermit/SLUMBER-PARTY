using UnityEngine;

public class FallingState : CharacterState
{
    public FallingState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        controller.animator.SetBool("isFalling", true);
        Debug.Log("Current State: Falling");
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        controller.animator.SetBool("isFalling", false);
    }

    // Called every frame
    public override void UpdateLogic() {
        if (controller.isGrounded() && controller.moveDirection.x < 0.01f)
        {
            controller.RequestIdle();
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        // Air control while falling
        controller.rb.linearVelocity = new Vector2(
            controller.moveDirection.x * controller.playerSpeed,
            Mathf.Max(controller.rb.linearVelocity.y, -controller.maxFallSpeed)
        );
    }
}
