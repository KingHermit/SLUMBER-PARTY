using Combat;
using Environment;
using System;
using System.Collections.Generic;
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
        DontDestroyOnLoad(gameObject);

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
        stateMap[stateMachine.CurrentStateID.Value].UpdateLogic();

        if (hitboxParent)
            hitboxParent.localScale = new Vector3(facing, 1, 1);
    }

    protected virtual void FixedUpdate()
    {
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
            NetworkVariableWritePermission.Owner
    );

    void UpdateFacing()
    {
        if (!IsOwner) return;
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
        if (moveIndex != -1) this.moveIndex = moveIndex;

        if (IsOwner)
        {
            // Debug.Log($"old moveIndex: {this.moveIndex}  |  new moveIndex: {moveIndex}"); // keeps switching back to -1. WHY WHY WHYYYYY
            stateMachine.ChangeState(id, this.moveIndex);
        }
        else if (!IsServer)
        {
            RequestStateChangeRpc(id, this.moveIndex);
        }
    }

    // idea: this method carries out server validation for switching states
    [Rpc(SendTo.Server, InvokePermission = RpcInvokePermission.Everyone)]
    public virtual void RequestStateChangeRpc(StateID requestedID, int moveIndex)
    {
        StateID current = stateMachine.CurrentStateID.Value;

        # region > SERVER VALIDATION

        if (wasLaunched) return;

        if (current == StateID.Attacking &&
            (requestedID == StateID.Idle || requestedID == StateID.Running))
        { return; }

        if (requestedID == StateID.Attacking && fallingThrough) return; // no attacking while passing through platform

        if (requestedID == StateID.Jumping && current == StateID.Stunned) return; // no jumping while stunned

        # endregion > SERVER VALIDATION

        ExecuteStateChangeRpc(requestedID, moveIndex);
    }


    // idea 2: this server actually tells the owner that requested the switch to carry out the change 👍
    [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Server)]
    private void ExecuteStateChangeRpc(StateID requestedID, int moveIndex)
    {
        stateMachine.ChangeState(requestedID, moveIndex);
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
        if (!IsOwner) return; // prevent duplicate calls from non-owners

        Vector2 dir = data.direction;
        dir.x *= packet.facing;
        dir.y = Mathf.Max(dir.y, 0.5f); // tweak this (0.3–0.8 feels good)

        //dir = dir.normalized;
        TakeDamageServerRpc(
            OwnerClientId,          // target
            data.damage,
            dir,
            data.knockbackForce,
            data.hitstunDuration
        );
    }

    [ServerRpc]
    public void TakeDamageServerRpc(ulong targetId, float damage, Vector2 dir, float force, float hitstun)
    {
        var target = NetworkManager.Singleton.ConnectedClients[targetId].PlayerObject;

        Debug.Log($"[Server] Before: {Health.Value}");

        Health.Value += damage;

        Debug.Log($"[Server] After: {Health.Value}");

        // Tell ONLY that client to apply knockback + hitstun
        ApplyHitEffectsClientRpc(targetId, dir, force, hitstun, Health.Value);
    }

    [ClientRpc]
    void ApplyHitEffectsClientRpc(ulong targetId, Vector2 dir, float force, float hitstun, float currentHealth)
    {
        if (NetworkManager.Singleton.LocalClientId != targetId)
            return;

        float scaledForce = force * (1f + currentHealth * 0.06f);
        hitstunTimer = hitstun;

        ApplyKnockback(dir, scaledForce); // VERTICAL KNOCKBACK NOT WORKING
        RequestHitstun();
    }

    public virtual void ApplyKnockback(Vector2 direction, float force)
    {
        wasLaunched = true;

        rb.AddForce(direction, ForceMode2D.Impulse);

        Debug.Log($"from {OwnerClientId}: Hit direction {direction} and {force}");
    }

    #endregion DAMAGE

    #region GETTERS & SETTERS

    public int getJumpLimit() => jumpLimit;

    #endregion GETTERS & SETTERS
}
