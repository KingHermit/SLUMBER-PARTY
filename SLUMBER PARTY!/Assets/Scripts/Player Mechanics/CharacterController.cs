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

    public NetworkVariable<Vector2> MoveDirection =
    new(Vector2.zero,
        NetworkVariableReadPermission.Everyone,
        NetworkVariableWritePermission.Owner);

    private int jumpLimit = 2;
    [SerializeField] protected int jumpsRemaining;
    public bool wasLaunched { get; set; }
    public float jumpForce { get; protected set; }
    public float maxFallSpeed = 60;
    public float fallThroughDuration = 0.3f;
    public bool fallingThrough { get; protected set; }




    #region INITIALIZATION

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
    }

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
            data.DamagePercent,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server);

        playerSpeed = data.Speed;
        jumpForce = data.JumpForce;
    }
    #endregion INITIALIZATION



    #region UPDATE

    protected virtual void Update()
    {
        if (!IsOwner) return;

        if (hitboxParent)
            hitboxParent.localScale = new Vector3(facing, 1, 1);

        stateMap[stateMachine.CurrentStateID.Value].UpdateLogic();
    }

    protected virtual void FixedUpdate()
    {
        if (!IsServer) return;

        UpdateFacing();
        stateMap[stateMachine.CurrentStateID.Value].UpdatePhysics();
    }

    protected virtual void LateUpdate()
    {
        GetComponent<SpriteRenderer>().flipX = !FacingRight.Value;
    }

    #endregion UPDATE

    public virtual bool isGrounded()
    {
        wasLaunched = false;

        Debug.DrawRay(new Vector2(transform.position.x, transform.position.y - 0.3f),
            Vector2.down * (GetComponent<SpriteRenderer>().bounds.extents.y - 0.8f),
            Color.red);

        bool groundCheck = Physics2D.Raycast(new Vector2(transform.position.x, transform.position.y - 0.3f),
            Vector2.down,
            GetComponent<SpriteRenderer>().bounds.extents.y - 0.8f, 
            LayerMask.GetMask("Platforms"));

        if (groundCheck) jumpsRemaining = getJumpLimit();

        return groundCheck;
    }


    #region --NETWORKING

    public NetworkVariable<float> Health;

    public NetworkVariable<bool> FacingRight =
        new NetworkVariable<bool>(
            true,
            NetworkVariableReadPermission.Everyone,
            NetworkVariableWritePermission.Server
    );

    void UpdateFacing()
    {
        if (MoveDirection.Value.x > 0 && !FacingRight.Value)
        {
            facing = 1;
            FacingRight.Value = true;
        } else if (MoveDirection.Value.x < 0 && FacingRight.Value)
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
        if (IsServer)
        {
            stateMachine.ChangeState(id, moveIndex);
        } else if (IsClient)
        {
            RequestStateChangeRpc(id);
        }
    }

    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public virtual void RequestStateChangeRpc(StateID requestedID)
    {
        // server-side validation[?]

        TransitionToState(requestedID); // old call
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
        if (!IsServer) return;

        Health.Value += data.damage;
        Debug.Log($"[Server] Resolving Hit on {gameObject.name}. Health before: {Health.Value}");

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
        // Debug.Log($"from {OwnerClientId}: Hit direction {direction} and {force}");

        wasLaunched = true;

        direction.x *= attackerFacing;

        rb.AddForce(direction * force, ForceMode2D.Impulse);
    }

    #endregion DAMAGE

    #region GETTERS & SETTERS

    public int getJumpLimit() => jumpLimit;

    #endregion GETTERS & SETTERS
}
