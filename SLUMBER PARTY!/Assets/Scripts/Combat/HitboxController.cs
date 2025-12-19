using UnityEngine;

namespace Combat
{
    public class HitboxController : MonoBehaviour
    {
        public HitboxData data;        // stores damage, size, angle, etc.
        public CharacterController owner; // who spawned the hitbox
        private BoxCollider2D box;

        private Vector2 localOffset;

        void Awake()
        {
            box = GetComponentInChildren<BoxCollider2D>();
            box.enabled = false; // start disabled
        }

        public void Setup(HitboxData hitboxData, CharacterController creator)
        {
            data = hitboxData;
            owner = creator;

            // Store the local offset so Update() can reposition it every frame
            localOffset = hitboxData.offset * owner.facing;


            // Set collider size
            box.size = hitboxData.size;
            box.offset = hitboxData.offset;

            // Snap to initial position
            Vector3 placedLocal = new Vector3(localOffset.x * owner.facing, localOffset.y * owner.facing, 0f);
            transform.localPosition = placedLocal;

            Enable();
        }

        void Update()
        {
            if (!active) return;

            // Make the hitbox follow the player (disjoint or attached)
            transform.localPosition = new Vector3(localOffset.x * owner.facing, localOffset.y * owner.facing, 0f);
        }

        private bool active;
        private void Enable()
        {
            // Activate
            active = true;
            box.enabled = true;
        }

        public void Disable()
        {
            // De-activate
            active = false;
            box.enabled = false;
        }

        void OnTriggerEnter2D(Collider2D other)
        {
            if (!active) return;
        }

        void OnDrawGizmos()
        {
            if (box == null) box = GetComponent<BoxCollider2D>();

            Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
            Gizmos.DrawWireCube(transform.position, box.size);
            Gizmos.DrawCube(transform.position, box.size);
        }
    }
}
