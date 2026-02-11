using Combat;
using System;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using Unity.Netcode;
using UnityEngine;


[SelectionBase]
public abstract class CharacterController : NetworkBehaviour
{
    // -- DATA --
    [SerializeField] public CharacterData data;

    // -- CORE --
    protected CharacterStateMachine stateMachine;
    public Rigidbody2D rb { get; protected set; }
    public AudioSource audioSource { get; protected set; }
    public Animator animator { get; protected set; }

    public AnimatorOverrideController animOverride;

    // -- STATES --
    protected Dictionary<StateID, CharacterState> stateMap;
    public int facing { get; protected set; } = 1;

    // -- COMBAT --
    public int moveIndex { get; protected set; }
    public Transform hitboxParent { get; protected set; }
    public bool isAttacking { get; protected set; } = false;
    public float hitstunTimer { get; protected set; }
    public bool isStunned { get; protected set; }

    // -- MOVEMENT --
    [SerializeField] public float playerSpeed { get; private set; }
    public Vector2 moveDirection { get; protected set; }
    public bool jumpPressed { get; protected set; }
    public bool wasLaunched { get; set; }
    public float jumpForce { get; protected set; }
    public float maxFallSpeed = 60;
    public float fallThroughDuration = 0.3f;
    public bool fallingThrough { get; protected set; }




    #region INITIALIZATION
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        stateMachine = GetComponent<CharacterStateMachine>();

        foreach (var hb in GetComponentsInChildren<HurtboxController>())
            hb.owner = this;

        animOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animOverride;
    }

    protected virtual void Start()
    {
        Health = new NetworkVariable<float>(
            data.maxHealth,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Owner);

        playerSpeed = data.speed;
        jumpForce = data.jumpForce;
    }
    #endregion INITIALIZATION



    #region UPDATE

    protected virtual void Update()
    {
        if (hitboxParent)
            hitboxParent.localScale = new Vector3(facing, 1, 1);

        if (hitstunTimer > 0f)
        {
            hitstunTimer -= Time.deltaTime;
            isStunned = true;
        } else
        {
            isStunned = false;
        }

        UpdateFacing();
        stateMap[stateMachine.CurrentStateID.Value].UpdateLogic();
    }

    protected virtual void FixedUpdate()
    {
        stateMap[stateMachine.CurrentStateID.Value].UpdatePhysics();
    }

    protected virtual void LateUpdate()
    {
        GetComponent<SpriteRenderer>().flipX = !FacingRight.Value;
    }

    #endregion UPDATE

    public virtual bool isGrounded()
    {
        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y - 0.3f),
            Vector2.down * (GetComponent<SpriteRenderer>().bounds.extents.y - 0.8f),
            Color.red);

        return Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - 0.3f),
            Vector2.down,
            GetComponent<SpriteRenderer>().bounds.extents.y - 0.8f, 
            LayerMask.GetMask("Platforms"));
    }


    #region --NETWORKING

    public NetworkVariable<float> Health;

    public NetworkVariable<bool> FacingRight =
    new NetworkVariable<bool>(
        true,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner
    );

    void UpdateFacing()
    {
        if (moveDirection.x > 0 && !FacingRight.Value)
        {
            facing = 1;
            FacingRight.Value = true;
        } else if (moveDirection.x < 0 && FacingRight.Value)
        {
            facing = -1;
            FacingRight.Value = false;
        }
    }

    #endregion --NETWORKING



    #region STATES

    // Universal transitions
    public virtual void TransitionToState(StateID id, int moveIndex = -1)
    {
        //Debug.Log("1. State changes");
        stateMachine.ChangeState(id, moveIndex);
    }

    public virtual void RequestStateChange(StateID requestedID)
    {
        // server-side validation
        if (stateMachine.CurrentStateID.Value == StateID.Stunned) return;

        TransitionToState(requestedID);
    }

    public virtual void RequestStateAttack(MovePacketNet moveNet)
    {
        // server-side validation
        if (stateMachine.CurrentStateID.Value == StateID.Stunned) return;

        moveIndex = moveNet.MoveIndex;

        TransitionToState(StateID.Attacking, moveIndex);
    }

    public abstract void RequestIdle();
    public abstract void RequestFall();
    public abstract void RequestHitstun();

    // Optional capabilities
    public virtual void RequestAttack(int moveIndex) { }
    public virtual void RequestJump() { }
    public virtual void RequestRun(Vector2 direction) { }

    #endregion STATES

    #region DAMAGE
    public virtual void setAttackTrue() // Only for PlayerController types
    {
        isAttacking = true;
    }

    public virtual void setAttackFalse() // Only for PlayerController types
    {
        isAttacking = false;
    }

    public virtual void OnHit(MovePacketNet packet) // TODO: FIX ATTACK AND KNOCKBACK AGAIN (MOVE TO HITSTUNSTATE)
    {
        if (IsServer) return;

        // Find the move and specific hitbox you were hit by 
        HitboxData hbData = data.moves[packet.MoveIndex].hitboxes[packet.HitboxIndex];

        Health.Value -= hbData.damage;

        hitstunTimer = hbData.hitstunDuration;

        Debug.Log("OW (DATA APPLIED");
        RequestHitstun();
        ApplyKnockback(hbData, packet.facing);
    }

    public virtual void ApplyKnockback(HitboxData data, int attackerFacing)
    {
        wasLaunched = true;
        Vector2 dir = data.direction.normalized;
        dir.x *= attackerFacing;

        rb.linearVelocity = dir * data.knockbackForce;
    }

    #endregion DAMAGE
}
