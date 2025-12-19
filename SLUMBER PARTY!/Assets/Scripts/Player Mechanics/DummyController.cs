using Combat;
using UnityEngine;

public class DummyController : CharacterController
{
    IdleState idle;
    HitstunState stunned;
    FallingState falling;

    protected override void Awake()
    {
        base.Awake();

        // TODO: Make sure CharacterState constructor takes in a plain CharacterController, not PlayerController!
        idle = new IdleState(this, stateMachine);
        stunned = new HitstunState(this, stateMachine);
        falling = new FallingState(this, stateMachine);
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idle);
    }

    #region STATE INTENT
    public override void RequestIdle()
    {
        stateMachine.ChangeState(idle);
    }

    public override void RequestFall()
    {
        stateMachine.ChangeState(falling);
    }

    public override void RequestHitstun(HitboxData hb)
    {
        stateMachine.ChangeState(stunned);
    }
    #endregion STATE INTENT

    public override void ApplyKnockback(HitboxController hb)
    {
        base.ApplyKnockback(hb);
        Debug.Log("Dummy launched!");
    }

    
}
