using Combat;
using Unity.Netcode;
using UnityEngine;

public class DummyController : CharacterController
{
    IdleState idle;
    HitstunState stunned;
    FallingState falling;

    protected override void Awake()
    {
        base.Awake();

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
        if (isStunned) return;
        if (!isGrounded()) return;
        stateMachine.ChangeState(idle);
    }

    public override void RequestFall()
    {
        if (isStunned) return;
        if (isGrounded()) return;
        stateMachine.ChangeState(falling);
    }

    public override void RequestHitstun(HitResult result)
    {
        stateMachine.ChangeState(stunned);
    }
    #endregion STATE INTENT

    #region DAMAGE

    
    public override void ApplyKnockback(HitResult result)
    {
        base.ApplyKnockback(result);
    }
    #endregion DAMAGE

}
