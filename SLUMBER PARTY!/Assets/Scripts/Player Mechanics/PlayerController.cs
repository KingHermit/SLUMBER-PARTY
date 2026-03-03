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
        RequestStateChange(StateID.Stunned);
    }

    public override void RequestIdle()
    {
        if (!isGrounded() || (stateMachine.CurrentStateID.Value == StateID.Stunned && isStunned)) return;

        RequestStateChange(StateID.Idle);
    }

    public override void RequestRun(Vector2 direction)
    {
        moveDirection = direction;

        if (stateMachine.CurrentStateID.Value == StateID.Attacking ||
            !isGrounded()) return;

        if (Mathf.Abs(moveDirection.x) > 0.01f)
        {
            RequestStateChange(StateID.Running);
            return;
        }
    }

    public override void RequestJump()
    {
        if ((stateMachine.CurrentStateID.Value == StateID.Attacking
            || !isGrounded())) return;
        jumpCount++;
        RequestStateChange(StateID.Jumping);
    }

    public override void RequestAttack(int moveIndex)
    {
        if (stateMachine.CurrentStateID.Value == StateID.Attacking) return;

        var packet = new MovePacketNet { MoveIndex = moveIndex };
        RequestStateAttack(packet);
    }

    public override void RequestFall()
    {
        if (isGrounded() || stateMachine.CurrentStateID.Value == StateID.Stunned) { return; }
        RequestStateChange(StateID.Falling);
    }

    #endregion STATE INTENT


    #region NETWORKING
    public override void OnNetworkSpawn()
    {
        input.enabled = IsOwner;
        cinemaCam.SetActive(IsOwner);

        stateMachine.CurrentStateID.OnValueChanged += (oldID, newID) =>
        {
            stateMachine.ChangeState(newID, moveIndex);
        };
    }

    public override void OnNetworkDespawn()
    {
        input.enabled = false;
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
