using Combat;
using System;
using System.Collections.Generic;
using Unity.Android.Gradle.Manifest;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.Diagnostics;


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
    public float hitstunTimer { get; set; }
    public bool isStunned { get; protected set; }

    // -- MOVEMENT --
    [SerializeField] public float playerSpeed { get; private set; }
    public Vector2 moveDirection { get; protected set; }
    public int jumpCount { get; protected set; }
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
            NetworkVariableWritePermission.Server);

        playerSpeed = data.speed;
        jumpForce = data.jumpForce;
    }
    #endregion INITIALIZATION



    #region UPDATE

    protected virtual void Update()
    {

        if (hitboxParent)
            hitboxParent.localScale = new Vector3(facing, 1, 1);

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
        jumpCount = 0;
        wasLaunched = false;

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
        stateMachine.ChangeState(id, moveIndex);
    }

    public virtual void RequestStateChange(StateID requestedID)
    {
        // server-side validation

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




    public virtual void ResolveHit(CharacterController attacker, HitboxData data, MovePacketNet packet)
    {
        if (!IsOwner)

        Health.Value -= data.damage;

        ApplyKnockback(
            data.direction.normalized,
            packet.facing,
            data.knockbackForce
            );
        RequestHitstun();
        hitstunTimer = data.hitstunDuration;
    }

    public virtual void ApplyKnockback(Vector2 direction, int attackerFacing, float force)
    {
        if (!IsOwner) return;
        // Debug.Log($"from {OwnerClientId}: Hit direction {direction} and {force}");

        wasLaunched = true;

        direction.x *= attackerFacing;

        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    #endregion DAMAGE
}
