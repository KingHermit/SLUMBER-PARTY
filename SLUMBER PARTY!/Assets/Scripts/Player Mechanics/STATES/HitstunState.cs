using UnityEngine;

public class HitstunState : CharacterState
{
    public HitstunState(CharacterController controller, CharacterStateMachine stateMachine)
        : base(controller, stateMachine) { }

    // Called ONCE when entering the state
    public override void Enter() {
        Debug.Log("I'VE BEEN HIT!!!");
    }

    // Called ONCE when exiting the state
    public override void Exit() {
        Debug.Log("Wait I'm okay now");
    }

    // Called every frame
    public override void UpdateLogic() { }
        
    // Called every physics frame
    public override void UpdatePhysics() { }
}
