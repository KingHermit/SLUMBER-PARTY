using Combat;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;

public class DummyController : CharacterController
{

    protected override void Awake()
    {
        base.Awake();

        stateMap = new Dictionary<StateID, CharacterState> {

            { StateID.Idle, new IdleState(this, stateMachine) },
            { StateID.Falling, new FallingState(this, stateMachine) },
            { StateID.Stunned, new HitstunState(this, stateMachine) },
        };
    }

    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(StateID.Idle, stateMap);
    }

    #region STATE INTENT
    public override void RequestIdle()
    {
        if (!isGrounded()) return;
        RequestStateChange(StateID.Idle);
    }

    public override void RequestFall()
    {
        if (isGrounded()) return;
        RequestStateChange(StateID.Falling);
    }

    public override void RequestHitstun()
    {
        RequestStateChange(StateID.Stunned);
    }
    #endregion STATE INTENT

    #region DAMAGE

    
    public override void ApplyKnockback(HitboxData data, int attackerFacing)
    {
        base.ApplyKnockback(data, attackerFacing);
    }
    #endregion DAMAGE

}
