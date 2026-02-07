using Combat;
using Unity.Netcode;
using UnityEngine;


[SelectionBase]
public abstract class CharacterController : NetworkBehaviour
{
    // -- DATA --
    [SerializeField] protected CharacterData data;

    // -- CORE --
    protected CharacterStateMachine stateMachine;
    public Rigidbody2D rb { get; protected set; }
    public AudioSource audioSource { get; protected set; }
    public Animator animator { get; protected set; }

    public AnimatorOverrideController animOverride;

    // -- STATS --
    public float health { get; protected set; }
    public int facing { get; protected set; } = 1;

    // -- COMBAT --
    public Transform hitboxParent { get; protected set; }
    public bool isAttacking { get; protected set; } = false;
    public bool isStunned { get; protected set; }
    public float hitstunTimer { get; protected set; }

    // -- MOVEMENT --
    [SerializeField] public float playerSpeed { get; private set; }
    public Vector2 moveDirection { get; protected set; }
    public bool jumpPressed { get; protected set; }
    public bool wasLaunched { get; set; }
    public float jumpForce { get; protected set; }
    public float maxFallSpeed = 45;
    public float fallThroughDuration = 0.3f;
    public bool fallingThrough { get; protected set; }

    #region INITIALIZATION
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();

        stateMachine = new CharacterStateMachine();

        foreach (var hb in GetComponentsInChildren<HurtboxController>())
            hb.owner = this;

        animOverride = new AnimatorOverrideController(animator.runtimeAnimatorController);
        animator.runtimeAnimatorController = animOverride;
    }

    protected virtual void Start()
    {
        health = data.maxHealth;
        playerSpeed = data.speed;
        jumpForce = data.jumpForce;
    }

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
        stateMachine.currentState.UpdateLogic();
    }

    protected virtual void FixedUpdate()
    {
        //if (PendingKnockback.Value != Vector2.zero)
        //{
        //    rb.linearVelocity = PendingKnockback.Value;
        //    PendingKnockback.Value = Vector2.zero;
        //}

        stateMachine.currentState.UpdatePhysics();
    }

    protected virtual void LateUpdate()
    {
        GetComponent<SpriteRenderer>().flipX = !FacingRight.Value;
    }

    public virtual bool isGrounded()
    {
        Debug.DrawRay(transform.position, Vector2.down * (GetComponent<SpriteRenderer>().bounds.extents.y - 0.5f), Color.red);
        return Physics2D.Raycast(transform.position,
            Vector2.down, GetComponent<SpriteRenderer>().bounds.extents.y - 0.5f, 
            LayerMask.GetMask("Platforms"));
    }
    #endregion INITIALIZATION

    #region NETWORKING

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

    // NOTE: Knockback should be an RPC, not a network variable!
    //public NetworkVariable<Vector2> PendingKnockback =
    //new NetworkVariable<Vector2>(
    //    Vector2.zero,
    //    NetworkVariableReadPermission.Everyone,
    //    NetworkVariableWritePermission.Owner
    //);

    #endregion

    #region STATES

    // Universal transitions
    public abstract void RequestIdle();
    public abstract void RequestFall();
    public abstract void RequestHitstun(HitResult result);

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

    public virtual void OnHit(ulong attackerID, HitResult result)
    {
        if (!IsServer) return;

        OnHitClientRpc(attackerID, result);
    }

    [Rpc(SendTo.ClientsAndHost)]
    public virtual void OnHitClientRpc(ulong attackerID, HitResult result)
    {
        health -= result.damage;
        hitstunTimer = result.hitstun;

        ApplyKnockback(result);
        RequestHitstun(result);
    }

    public virtual void ApplyKnockback(HitResult result)
    {
        wasLaunched = true;

        Vector2 dir = result.direction.normalized;

        dir.x *= result.attackerFacing;

        Vector2 force = dir * result.knockbackForce;
        rb.linearVelocity = force;
    }

    #endregion DAMAGE
}
