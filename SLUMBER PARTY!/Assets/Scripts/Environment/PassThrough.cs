using System;
using System.Collections;
using UnityEngine;

public class NewMonoBehaviourScript : MonoBehaviour
{
    private Collider2D _collider;
    private Collider2D _playerCol;
    private bool playerOnPlatform;

    private void Start()
    {
        _collider = GetComponent<Collider2D>();
        _playerCol = GameObject.FindWithTag("Player").GetComponent<Collider2D>();
    }

    private void Update()
    {
        if (playerOnPlatform && Input.GetAxisRaw("Vertical") < 0)
        {
            Physics2D.IgnoreCollision(_collider, _playerCol, true);
            StartCoroutine(EnableCollider());
        }
    }

    private IEnumerator EnableCollider()
    {
        yield return new WaitForSeconds(0.3f);
        Physics2D.IgnoreCollision(_collider, _playerCol, false);
    }

    private void SetPlayerOnPlatform(Collision2D other, bool value)
    {
        var player = other.gameObject.GetComponent<PlayerController>();

        if (player != null)
        {
            playerOnPlatform = value;
        }
    }

    private void OnCollisionEnter2D(Collision2D other)
    {
        SetPlayerOnPlatform(other, true);
    }

    private void OnCollisionExit2D(Collision2D other)
    {
        SetPlayerOnPlatform(other, false);
    }
}
