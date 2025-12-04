using UnityEngine;

public class HitstunState : PlayerState
{
    public HitstunState(PlayerController player, PlayerStateMachine stateMachine)
        : base(player, stateMachine) { }

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
