using UnityEngine;

public class FallingState : CharacterState
{
    float decayMultiplier = 1f;

    public FallingState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        controller.animator.SetBool("isFalling", true);
        if (controller is DummyController) Debug.Log($"{controller.name} current State: Falling");

        if (controller.wasLaunched)
        {
            decayMultiplier = 0.2f;
            controller.wasLaunched = false;
        } else
        {
            decayMultiplier = 1f;
        }
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        controller.animator.SetBool("isFalling", false);
    }

    // Called every frame
    public override void UpdateLogic() {
        if (controller.isGrounded())
        {
            controller.RequestIdle();
            return;
        }
    }

    // Called every physics frame
    public override void UpdatePhysics() {
        // Air control while falling
        float x = Mathf.Lerp(
            controller.rb.linearVelocity.x,
            0f,
            decayMultiplier * Time.fixedDeltaTime
            );

        float y = Mathf.Max(
            controller.rb.linearVelocity.y,
            -controller.maxFallSpeed
            );

        controller.rb.linearVelocity = new Vector2(x, y);
    }
}
