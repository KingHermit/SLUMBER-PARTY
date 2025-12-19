using UnityEngine;
using UnityEngine.EventSystems;

public class RunningState : CharacterState
{
    public RunningState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        controller.animator.SetBool("isRunning", true);
        Debug.Log("Current state: Running");
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        controller.animator.SetBool("isRunning", false);
    }

    // Called every frame
    public override void UpdateLogic() {

        // Idle Transition
        if (controller.moveDirection.x == 0)
        {
            controller.RequestIdle();
            return;
        }

        // Falling Transition
        if (!controller.isGrounded())
        {
            controller.RequestFall();
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        controller.rb.linearVelocity = new Vector2(
        controller.moveDirection.x * controller.playerSpeed,
        controller.rb.linearVelocity.y
    );
    }
}
