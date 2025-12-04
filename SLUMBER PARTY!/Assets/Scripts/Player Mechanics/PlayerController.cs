using NUnit.Framework.Interfaces;
using System;
using System.Collections;
using Unity.IO.LowLevel.Unsafe;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerController : MonoBehaviour
{
    // -- CHARACTER DATA --
    [SerializeField] private CharacterData data;
    private PlayerStateMachine playerStateMachine;
    public Transform hitboxParent;

    [Header("STATS")]
    public float health { get; private set; }

    // -- COMPONENTS --
    [HideInInspector] public Rigidbody2D rb;
    public InputActionReference m_input;
    public Animator _animator;

    // -- PLAYER STATES --
    public IdleState idle;
    public RunningState running;
    public JumpingState jumping;
    public FallingState falling;
    public AttackState attacking;
    public HitstunState stunned;

    // -- MOVEMENT STATS --
    [Header("Movement")]
    public float playerSpeed { get; private set; }
    public Vector2 _moveDirection {  get; private set; }
    public int facing { get; private set; }

    // -- JUMPING --
    [Header("Jumping")]
    [HideInInspector] public bool jumpPressed;
    [SerializeField] public float jumpForce { get; private set; }
    public float maxFallSpeed = 45;

    // -- GROUND CHECKING --
    [Header("Ground Check")]
    private PlatformEffector2D effector;

    // -- PHASING THROUGH --
    [Header("Drop / Phase Through")]
    [SerializeField] private float fallThroughDuration = 0.3f;

    // -- BOOLEANS --
    public bool isAttacking;
    public bool fallingThrough { get; private set; }

    private void Awake()
    {
        foreach (var hb in GetComponentsInChildren<HurtboxController>())
            hb.owner = this; // set owner properly

        rb = GetComponent<Rigidbody2D>();
        _animator = GetComponent<Animator>();

        playerStateMachine = new PlayerStateMachine();
        hitboxParent = transform.Find("HITBOXES");
        // hitboxPrefab = Resources.Load<GameObject>("hitbox");

        idle = new IdleState(this, playerStateMachine);
        running = new RunningState(this, playerStateMachine);
        jumping = new JumpingState(this, playerStateMachine);
        falling = new FallingState(this, playerStateMachine);
        attacking = new AttackState(this, playerStateMachine);
        stunned = new HitstunState(this, playerStateMachine);
    }

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        playerStateMachine.Initialize(idle);
        Debug.Log("Character reporting for duty: " + data.name);

        health = data.maxHealth;
        playerSpeed = data.speed;
        jumpForce = data.jumpForce;
    }

    private IEnumerator FallThroughPlatform()
    {
        effector = GetCurrentPlatformEffector();

        fallingThrough = true;
        effector.rotationalOffset = 180f;

        rb.linearVelocity = new Vector2(rb.linearVelocity.y, -5f); // apply downward force

        yield return new WaitForSeconds(fallThroughDuration);

        effector.rotationalOffset = 0;
        fallingThrough = false;
    }

    // Update is called once per frame
    void Update()
    {
        // flip
        if (_moveDirection.x != 0)
        {
            facing = (int)_moveDirection.x;
        }
        hitboxParent.transform.localScale = new Vector3(facing, 1f, 1f);

        playerStateMachine.currentState.UpdateLogic();
    }

    private void FixedUpdate()
    {
        playerStateMachine.currentState.UpdatePhysics();
    }
    public CharacterData GetCharData()
    {
        return data;
    }

    public bool isGrounded()
    {
        return Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Platforms"));
    }

    private PlatformEffector2D GetCurrentPlatformEffector()
    {
        RaycastHit2D hit = Physics2D.Raycast(transform.position, Vector2.down, 1.2f, LayerMask.GetMask("Platforms"));
        if (hit.collider != null)
            return hit.collider.GetComponent<PlatformEffector2D>();
        return null;
    }

    public void OnHit(HitboxController hb)
    {
        // apply damage
        health -= hb.data.damage;

        // apply knockback
        Vector2 kb = hb.data.direction.normalized * hb.data.knockbackForce;
        kb.x *= hb.owner.facing; // flip
        rb.linearVelocity = kb;

        // enter hitstun state
        playerStateMachine.ChangeState(stunned);
    }

    // -- INPUT SYSTEM CALLBACKS --

    public void OnLightAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            isAttacking = true;
            playerStateMachine.ChangeState(attacking, data.moves[0]);
        }
    }

    public void OnMediumAttack(InputAction.CallbackContext context)
    {
        if (context.performed && !isAttacking)
        {
            isAttacking = true;
            playerStateMachine.ChangeState(attacking, data.moves[1]);
        }
    }

    public void OnHeavyAttack(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            isAttacking = true;
            playerStateMachine.ChangeState(attacking, data.moves[2]);
        }
    }

    public void OnMove(InputAction.CallbackContext context)
    {
        _moveDirection = m_input.action.ReadValue<Vector2>();
        if (isGrounded()) { playerStateMachine.ChangeState(running); }
    }

    public void OnJump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded())
        {
            playerStateMachine.ChangeState(jumping);
        }
    }

    public void OnDrop(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded())
        {
            playerStateMachine.ChangeState(falling);
            StartCoroutine("FallThroughPlatform");
        }
    }
}
