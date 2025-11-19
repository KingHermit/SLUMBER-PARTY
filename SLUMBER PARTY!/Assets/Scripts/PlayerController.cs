using System;
using UnityEngine;

public class PlayerController : MonoBehaviour
{
    [SerializeField] private BoxCollider2D b_collider;
    [SerializeField] private Rigidbody2D rb;

    // -- CHARACTER DATA
    [SerializeField] public CharacterData data;

    // -- MOVEMENT STATS --
    [SerializeField]
    private Vector2 _movement;
    private float playerSpeed = 2f;
    private float jumpForce;

    // Start is called once before the first execution of Update after the MonoBehaviour is created
    void Start()
    {
        rb = GetComponent<Rigidbody2D>();
    }

    // Update is called once per frame
    void Update()
    {
        _movement = new Vector2(Input.GetAxis("Horizontal"), Input.GetAxis("Vertical")); 
        HandleMovement(_movement);
    }

    private void HandleMovement(Vector2 direction)
    {
        rb.AddForce(direction * playerSpeed);

        if (Input.GetKeyDown(KeyCode.Space))
        {
            Console.WriteLine("Jumpingggg");
        }
        
    }
}
