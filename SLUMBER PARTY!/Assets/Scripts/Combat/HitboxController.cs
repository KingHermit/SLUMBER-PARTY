using UnityEngine;

public class HitboxController : MonoBehaviour
{
    public HitboxData data;        // stores damage, size, angle, etc.
    public PlayerController owner; // who spawned the hitbox
    private BoxCollider2D box;

    private bool active;

    private Vector2 localOffset;

    void Awake()
    {
        box = GetComponent<BoxCollider2D>();
        box.enabled = false; // start disabled
    }

    public void Setup(HitboxData hitboxData, PlayerController creator)
    {
        data = hitboxData;
        owner = creator;

        // Store the local offset so Update() can reposition it every frame
        localOffset = hitboxData.offset;

        // Set collider size
        box.size = hitboxData.size;

        // Activate
        active = true;
        box.enabled = true;

        // Snap to initial position
        transform.position = owner.transform.position + (Vector3)localOffset;
    }

    void Update()
    {
        if (!active) return;

        // Make the hitbox follow the player (disjoint or attached)
        transform.position = owner.transform.position + (Vector3)localOffset;
    }

    void OnTriggerEnter2D(Collider2D other)
    {
        if (!active) return;

        // Prevent hitting self
        if (other.gameObject == owner.gameObject) return;

        //var hurtbox = other.GetComponent<Hurtbox>();
        //if (hurtbox)
        //{
        //    hurtbox.TakeHit(data, owner);
        //    Disable();
        //}
    }
}
