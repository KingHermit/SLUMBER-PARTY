using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : CharacterController
{
    // -- PLAYER STATES --
    [Header("States")]
    
    [Header("Ground Check")]
    private PlatformEffector2D effector;

    [SerializeField] PlayerInput input;
    [SerializeField] GameObject cinemaCam;

    protected override void Awake()
    {
        base.Awake();

        input = GetComponent<PlayerInput>();
        hitboxParent = transform.Find("HITBOXES");

        stateMap = new Dictionary<StateID, CharacterState> {

            { StateID.Idle, new IdleState(this, stateMachine) },
            { StateID.Running, new RunningState(this, stateMachine) },
            { StateID.Jumping, new JumpingState(this, stateMachine) },
            { StateID.Falling, new FallingState(this, stateMachine) },
            { StateID.Attacking, new AttackState(this, stateMachine) },
            { StateID.Stunned, new HitstunState(this, stateMachine) },
        };

        stateMachine.Initialize(StateID.Idle, stateMap);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();

        Debug.Log("Character reporting for duty: " + data.name);
    }

    private IEnumerator FallThroughPlatform()
    {
        effector = GetCurrentPlatformEffector();

        fallingThrough = true;
        effector.rotationalOffset = 180f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.y, -6f); // apply downward force

        yield return new WaitForSeconds(fallThroughDuration);

        effector.rotationalOffset = 0;
        fallingThrough = false;
    }

    private PlatformEffector2D GetCurrentPlatformEffector()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Platforms"));
        if (hit.collider != null)
            return hit.collider.GetComponent<PlatformEffector2D>();
        return null;
    }

    // -- STATE INTENT --
    #region STATE INTENT

    public override void RequestHitstun()
    {
        TransitionToState(StateID.Stunned);
    }

    public override void RequestIdle()
    {
        if (!isGrounded()) { return; }
        if (stateMachine.CurrentStateID.Value == StateID.Stunned) { return; }

        TransitionToState(StateID.Idle);
    }

    public override void RequestRun(Vector2 direction)
    {
        if (!IsOwner) return;

        MoveDirection.Value = direction;

        if (stateMachine.CurrentStateID.Value == StateID.Attacking ||
            !isGrounded()) return;

        if (Mathf.Abs(MoveDirection.Value.x) > 0.01f)
        {
            TransitionToState(StateID.Running);
        } else
        {
            TransitionToState(StateID.Idle);
        }
    }

    public override void RequestJump()
    {
        if (stateMachine.CurrentStateID.Value == StateID.Attacking || jumpsRemaining <= 0) return;

        TransitionToState(StateID.Jumping);
        jumpsRemaining -= 1;
    }

    public override void RequestAttack(int moveIndex)
    {
        if (stateMachine.CurrentStateID.Value == StateID.Attacking) return;

        //var packet = new MovePacketNet { MoveIndex = moveIndex };
        //RequestStateAttack(packet);
        TransitionToState(StateID.Attacking, moveIndex);
    }

    public override void RequestFall()
    {
        if (isGrounded()) { return; }

        if (stateMachine.CurrentStateID.Value == StateID.Stunned) { return; }
        TransitionToState(StateID.Falling);
    }

    #endregion STATE INTENT


    #region NETWORKING
    public override void OnNetworkSpawn()
    {
        input.enabled = IsOwner;
        cinemaCam.SetActive(IsOwner);

        if (IsClient)
        {
            stateMachine.CurrentStateID.OnValueChanged += (oldID, newID) =>
            {
                //TransitionToState(newID, moveIndex);
                stateMachine.ChangeState(newID, moveIndex);
            };

            // subscribe to attack index changes soon
            stateMachine.CurrentAttackIndex.OnValueChanged += (oldIdx, newIdx) =>
            {
                stateMachine.ChangeAttackIndex(newIdx);
            };
        }
    }

    public override void OnNetworkDespawn()
    {
        input.enabled = false;

        if (IsClient)
        {
            stateMachine.CurrentStateID.OnValueChanged -= (oldID, newID) =>
            {
                //TransitionToState(newID, moveIndex);
                stateMachine.ChangeState(newID, moveIndex);
            };

            stateMachine.CurrentAttackIndex.OnValueChanged += (oldIdx, newIdx) =>
            {
                stateMachine.ChangeAttackIndex(newIdx);
            };
        }
    }

    #endregion NETWORKING


    // -- INPUT SYSTEM CALLBACKS --
    #region INPUT SYSTEM CALLBACKS
    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        moveIndex = 0;
        RequestAttack(moveIndex);
    }

    public void OnMediumAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        moveIndex = 1;
        RequestAttack(moveIndex);
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        moveIndex = 2;
        RequestAttack(moveIndex);
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        Vector2 input = context.ReadValue<Vector2>();
        RequestRun(input);
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        RequestJump();
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (!context.performed) return;

        if (!fallingThrough)
        {
            StartCoroutine("FallThroughPlatform");
        }
    }

    #endregion INPUTSYSTEMCALLBACKS

    #region DAMAGE

    #endregion DAMAGE
}
