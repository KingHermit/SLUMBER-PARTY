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
    public float jumpForce { get; protected set; }
    public float maxFallSpeed = 45;
    public float fallThroughDuration = 0.3f;
    public bool fallingThrough { get; protected set; }

    #region NETWORKING

    #endregion

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
        if (moveDirection.x != 0)
            facing = (int)Mathf.Sign(moveDirection.x);

        if (facing > 0)
        {
            GetComponent<SpriteRenderer>().flipX = false;
        } else
        {
            GetComponent<SpriteRenderer>().flipX = true;
        }

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

        stateMachine.currentState.UpdateLogic();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.currentState.UpdatePhysics();
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

    #endregion

    #region STATES

    // Universal transitions
    public abstract void RequestIdle();
    public abstract void RequestFall();
    public abstract void RequestHitstun(HitboxData hb);

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

    public void ClearStun()
    {
        isStunned = false;
    }

    public virtual void OnHit(HitboxController hb)
    {   
        // apply damage
        health -= hb.data.damage;

        // enter hitstun state
        hitstunTimer = hb.data.hitstunDuration;

        ApplyKnockback(hb);
        RequestHitstun(hb.data);
    }

    public virtual void ApplyKnockback(HitboxController hb)
    {
        Vector2 dir = hb.data.direction.normalized;

        dir.x *= hb.owner.facing;

        Vector2 force = dir * hb.data.knockbackForce;

        rb.linearVelocity = force;
    }
    #endregion DAMAGE
}
