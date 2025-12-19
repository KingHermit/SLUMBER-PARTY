using Combat;
using Unity.VisualScripting;
using UnityEngine;

public abstract class CharacterController : MonoBehaviour
{
    // -- DATA --
    [SerializeField] protected CharacterData data;

    // -- CORE --
    protected CharacterStateMachine stateMachine;
    public Rigidbody2D rb { get; protected set; }
    public Animator animator { get; protected set; }

    // -- STATS --
    public float health { get; protected set; }
    public int facing { get; protected set; } = 1;

    // -- COMBAT --
    public Transform hitboxParent { get; protected set; }
    public bool isAttacking { get; protected set; } = false;

    // -- MOVEMENT --
    [SerializeField] public float playerSpeed { get; private set; }
    public Vector2 moveDirection { get; protected set; }
    public bool jumpPressed { get; protected set; }
    public float jumpForce { get; protected set; }
    public float maxFallSpeed = 45;
    public float fallThroughDuration = 0.3f;
    public bool fallingThrough { get; protected set; }

    #region INITIALIZATION
    protected virtual void Awake()
    {
        rb = GetComponent<Rigidbody2D>();
        animator = GetComponent<Animator>();

        stateMachine = new CharacterStateMachine();

        foreach (var hb in GetComponentsInChildren<HurtboxController>())
            hb.owner = this;
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

        if (hitboxParent)
            hitboxParent.localScale = new Vector3(facing, 1, 1);

        stateMachine.currentState.UpdateLogic();
    }

    protected virtual void FixedUpdate()
    {
        stateMachine.currentState.UpdatePhysics();
    }

    public virtual bool isGrounded()
    {
        return Physics2D.Raycast(transform.position,
            Vector2.down, 1.2f, 
            LayerMask.GetMask("Platforms"));
    }
    #endregion INITIALIZATION

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
    public virtual void setAttackTrue()
    {
        isAttacking = true;
    }

    public virtual void setAttackFalse()
    {
        isAttacking = false;
    }

    public virtual void OnHit(HitboxController hb)
    {
        // apply damage
        health -= hb.data.damage;

        ApplyKnockback(hb);

        // enter hitstun state
        // stateMachine.ChangeState(stunned);
    }

    public virtual void ApplyKnockback(HitboxController hb)
    {
        Vector2 kb = hb.data.direction.normalized * hb.data.knockbackForce;
        kb.x *= hb.owner.facing;

        rb.linearVelocity = kb;
    }
    #endregion DAMAGE
}
