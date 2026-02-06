using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using System.Collections.Generic;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.TextCore.Text;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerController : CharacterController
{
    [SerializeField] public PlayerInput input;

    // -- PLAYER STATES --
    [Header("States")]
    public IdleState idle;
    public RunningState running;
    public JumpingState jumping;
    public FallingState falling;
    public AttackState attacking;
    public HitstunState stunned;

    [Header("Ground Check")]
    private PlatformEffector2D effector;

    protected override void Awake()
    {
        base.Awake();

        input = GetComponent<PlayerInput>();
        hitboxParent = transform.Find("HITBOXES");

        idle = new IdleState(this, stateMachine);
        running = new RunningState(this, stateMachine);
        jumping = new JumpingState(this, stateMachine);
        falling = new FallingState(this, stateMachine);
        attacking = new AttackState(this, stateMachine);
        stunned = new HitstunState(this, stateMachine);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    protected override void Start()
    {
        base.Start();
        stateMachine.Initialize(idle);

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
    public override void RequestIdle()
    {
        if (isStunned) return;
        stateMachine.ChangeState(idle);
    }

    public override void RequestRun(Vector2 direction)
    {
        moveDirection = direction;

        if (isAttacking) return;
        if (isStunned) return;
        if (!isGrounded()) return;

        if (Mathf.Abs(moveDirection.x) > 0.01f)
        {
            stateMachine.ChangeState(running);
        } else
        {
            RequestIdle();
        }
    }

    public override void RequestJump()
    {
        if (isStunned) return;
        if (!isGrounded()) { return; }
        if (isAttacking) { return; }

        stateMachine.ChangeState(jumping);
    }

    public override void RequestAttack(int moveIndex)
    {
        if (isStunned) return;
        if (isAttacking) { return; }
        // if (!isGrounded()) return; // optional rule

        stateMachine.ChangeState(attacking, data.moves[moveIndex]);
    }

    public override void RequestFall()
    {
        if (isStunned) return;
        if (isGrounded()) { return; }
        stateMachine.ChangeState(falling);
    }

    public override void RequestHitstun(HitboxData hb)
    {
        stateMachine.ChangeState(stunned);
    }
    #endregion STATE INTENT


    // -- INPUT SYSTEM CALLBACKS --
    #region INPUT SYSTEM CALLBACKS
    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        RequestAttack(0);
    }

    public void OnMediumAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        RequestAttack(1);
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (!context.performed) return;
        RequestAttack(2);
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
