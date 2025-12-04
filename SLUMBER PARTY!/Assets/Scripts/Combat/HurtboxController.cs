using UnityEngine;
using UnityEngine.UIElements;

public class HurtboxController : MonoBehaviour
{
    public MonoBehaviour owner;
    private BoxCollider2D box;

    private void Start()
    {
        box = GetComponent<BoxCollider2D>();
        foreach (var hb in GetComponentsInChildren<HurtboxController>())
            hb.owner = this; // set owner properly
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        var hitbox = collision.GetComponent<HitboxController>();

        if (hitbox == null)
        {
            // Optional debug, kept safe
            Debug.Log("Hurtbox collided with non-hitbox object.");
            return;
        }

        if (hitbox.owner == owner) return; // don't hit yourself!!!

        if (owner is PlayerController p)
        {
            p.OnHit(hitbox);
        }

        Debug.Log($"Hurtbox hit by hitbox: {hitbox.data.name}");
    }

    void OnDrawGizmos()
    {
        if (box == null) box = GetComponent<BoxCollider2D>();

        Gizmos.color = new Color(0f, 1f, 1f, 0.35f);
        Gizmos.DrawWireCube(transform.position, box.size);
        Gizmos.DrawCube(transform.position, box.size);
    }
}
