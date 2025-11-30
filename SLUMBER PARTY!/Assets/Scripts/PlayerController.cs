using System;
using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerController : MonoBehaviour
{
    // -- CHARACTER DATA --
    [SerializeField] private CharacterData data;

    // -- COMPONENTS
    [SerializeField] private BoxCollider2D b_collider;
    [SerializeField] private Rigidbody2D rb;
    public InputActionReference move;

    // -- MOVEMENT STATS --
    [Header("Movement")]
    [SerializeField] private float playerSpeed;
    private Vector2 _moveDirection;

    [Header("Jumping")]
    [SerializeField] private float jumpForce;

    [Header("Ground Check")]
    private PlatformEffector2D effector;

    [Header("Drop / Phase Through")]
    [SerializeField] private float fallThroughDuration = 0.3f;
    private bool fallingThrough = false;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Debug.Log("Character reporting for duty: " + data.name);

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
        if (isGrounded() && Input.GetAxisRaw("Vertical") < 0 && !fallingThrough)
        {
            StartCoroutine(FallThroughPlatform());
        }
    }

    public void Jump(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded())
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    public void Move(InputAction.CallbackContext context)
    {
        _moveDirection = move.action.ReadValue<Vector2>();
    }

    public void Drop(InputAction.CallbackContext context)
    {
        if (context.performed && isGrounded())
        {
            StartCoroutine("FallThroughPlatform");
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(_moveDirection.x * playerSpeed, rb.linearVelocity.y);
    }

    private Boolean isGrounded()
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

}
