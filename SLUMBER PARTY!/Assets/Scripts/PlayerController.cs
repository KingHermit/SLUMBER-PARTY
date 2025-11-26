using System;
using UnityEngine;
using static UnityEditor.Searcher.SearcherWindow.Alignment;

public class PlayerController : MonoBehaviour
{
    // -- CHARACTER DATA --
    [SerializeField] private CharacterData data;

    // -- COMPONENTS
    [SerializeField] private BoxCollider2D b_collider;
    [SerializeField] private Rigidbody2D rb;

    // -- MOVEMENT STATS --
    [SerializeField]
    private float horizontal;
    private float playerSpeed;
    private float jumpForce;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();

        Debug.Log("Character reporting for duty: " + data.name);

        playerSpeed = data.speed;
        jumpForce = data.jumpForce;
    }

    // Update is called once per frame
    void Update()
    {
        horizontal = Input.GetAxisRaw("Horizontal");

        // temp jumping controls
        if (Input.GetKeyDown(KeyCode.Space))
        {
            rb.linearVelocity = new Vector2(rb.linearVelocity.x, jumpForce);
        }
    }

    private void FixedUpdate()
    {
        rb.linearVelocity = new Vector2(horizontal * playerSpeed, rb.linearVelocity.y);
    }

    private Boolean isGrounded()
    {
        return rb.linearVelocity.y < 1;
    }
}
