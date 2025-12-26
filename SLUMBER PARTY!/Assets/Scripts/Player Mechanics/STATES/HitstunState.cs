using UnityEngine;

public class HitstunState : CharacterState
{
    public HitstunState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    float stunDuration;

    // Called ONCE when entering the state
    public override void Enter() {
        Debug.Log($"{controller.name} current state: Stunned");
        controller.animator.SetBool("isStunned", true);
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        controller.ClearStun();
        controller.animator.SetBool("isStunned", false);
    }

    // Called every frame
    public override void UpdateLogic() {
        if (!controller.isStunned) { // is exiting hitstun state before the timer. What's wrong?
            Debug.Log("I'M OKAY!");
            if (!controller.isGrounded()) { 
                controller.RequestFall();
            } else
            {
                controller.RequestIdle();
            }
        }
    }
        
    // Called every physics frame
    public override void UpdatePhysics() { }
}
