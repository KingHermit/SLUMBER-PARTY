using UnityEngine;

public class JumpingState : CharacterState
{
    public JumpingState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        controller.animator.SetBool("isJumping", true);

        // jump ONE time
        controller.rb.linearVelocity = new Vector2(
            controller.rb.linearVelocity.x,
            controller.jumpForce
        );

        // Debug.Log($"{controller.name} current State: Jumping");
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        controller.animator.SetBool("isJumping", false);
    }

    // Called every frame
    public override void UpdateLogic() {
        if (controller.rb.linearVelocityY < 0)
        {
            controller.RequestFall();
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        // air horizontal control
        controller.rb.linearVelocity = new Vector2(
            controller.moveDirection.x * controller.playerSpeed,
            controller.rb.linearVelocity.y // <-- LEAVE Y ALONE!!!
        );
    }
}
