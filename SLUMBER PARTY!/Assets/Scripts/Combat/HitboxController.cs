using Environment;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Combat
{
    public class HitboxController : MonoBehaviour
    {
        public HitboxData data;        // stores damage, size, angle, etc.
        public CharacterController owner; // who spawned the hitbox
        private BoxCollider2D box;

        private int moveIndex;
        private int hitboxIndex;

        private Vector2 localOffset;

        void Awake()
        {
            box = GetComponentInChildren<BoxCollider2D>();
            box.enabled = false; // start disabled
        }

        public void Setup(HitboxData hitboxData, CharacterController creator, int moveIdx, int hitboxIdx)
        {
            data = hitboxData;
            owner = creator;
            moveIndex = moveIdx;
            hitboxIndex = hitboxIdx;

            // Store the local offset so Update() can reposition it every frame
            localOffset = hitboxData.offset * owner.facing;

            // Set collider size
            box.size = hitboxData.size;

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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (collision.TryGetComponent(out HurtboxController hurtboxController))
            {
                if (!owner.IsOwner) return;

                var hurtbox = collision.GetComponent<HurtboxController>();
                if (hurtbox == null) return;
                if (hurtbox.owner == owner) return;

                Debug.Log($"Trigger on {hurtbox.owner.OwnerClientId} | isServer={hurtbox.owner.IsServer} isOwner={hurtbox.owner.IsOwner}");

                MovePacketNet packet = new MovePacketNet
                {
                    attackerID = owner.NetworkObjectId,
                    MoveIndex = moveIndex,
                    HitboxIndex = hitboxIndex,
                    facing = owner.facing
                };

                setHitAudio(hurtbox.owner, data.audio);

                hurtbox.ReportHitServerRpc(
                    owner.OwnerClientId,
                    packet
                );
            }
        }

        private void setHitAudio(CharacterController victim, AudioClip hitClip)
        {
            victim.audioSource.clip = hitClip;
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

        void OnDrawGizmos()
        {
            if (box == null) box = GetComponent<BoxCollider2D>();

            Gizmos.color = new Color(1f, 0f, 0f, 0.35f);
            Gizmos.DrawWireCube(transform.position, box.size);
            Gizmos.DrawCube(transform.position, box.size);
        }
    }
}
