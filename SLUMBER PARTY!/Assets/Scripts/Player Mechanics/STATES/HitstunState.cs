using Unity.VisualScripting;
using UnityEngine;

public class HitstunState : CharacterState
{
    public HitstunState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        // Debug.Log($"{controller.OwnerClientId} current state: Stunned");
        controller.animator.SetBool("isStunned", true);
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        controller.animator.SetBool("isStunned", false);
    }

    // Called every frame
    public override void UpdateLogic() {

        controller.hitstunTimer -= Time.deltaTime;

        if (controller.hitstunTimer < 0)
        {
            if (!controller.isGrounded())
            {
                stateMachine.ChangeState(StateID.Falling);
                return;
            } else
            {
                stateMachine.ChangeState(StateID.Idle);
                return;
            }
        }
    }
        
    // Called every physics frame
    public override void UpdatePhysics() {}
}
