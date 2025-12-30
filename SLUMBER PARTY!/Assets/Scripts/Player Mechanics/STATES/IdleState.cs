using UnityEngine;

public class IdleState : CharacterState
{
    public IdleState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        // Debug.Log($"{controller.name} current state: Idle");
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        
    }

    // Called every frame
    public override void UpdateLogic() {
        // Run Transition
        if (Mathf.Abs(controller.moveDirection.x) != 0 && controller.isGrounded())
        {
            controller.RequestRun(controller.moveDirection);
            return;
        }

        // Falling transition
        if (!controller.isGrounded())
        {
            controller.RequestFall();
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        controller.rb.linearVelocity = new Vector2(
        controller.moveDirection.x,
        controller.rb.linearVelocity.y
    );
    }
}
