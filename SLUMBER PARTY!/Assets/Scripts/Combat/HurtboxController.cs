using Environment;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Combat
{
    public class HurtboxController : MonoBehaviour
    {
        // HitboxController hitbox;
        public CharacterController owner;
        private BoxCollider2D box;

        private void Start()
        {
            box = GetComponent<BoxCollider2D>();
            box.isTrigger = true;

            owner = GetComponentInParent<CharacterController>();

            if (owner == null) {
                Debug.LogError("Hurtbox has no CharacterController owner!", this);
            }
        }

        [Rpc(SendTo.Server)]
        public void ReportHitServerRpc(CharacterController attacker, MovePacketNet packet)
        {
            // INSERT SERVER VALIDATION LATER
            HitboxData hbData = owner.data.moves[packet.MoveIndex].hitboxes[packet.HitboxIndex];

            PlayHitEffectsClientRpc(hbData);
            owner.OnHitRpc(packet.attackerID, packet);
        }


        [ClientRpc]
        private void PlayHitEffectsClientRpc(HitboxData data)
        {
            owner.audioSource.Play();
            FindAnyObjectByType<Hitstop>().Stop(data.hitstopDuration);
        }

        void OnDrawGizmos()
        {
            if (box == null) box = GetComponent<BoxCollider2D>();

            Gizmos.color = new Color(0f, 1f, 1f, 0.15f);
            Gizmos.DrawWireCube(transform.position, box.size);
            Gizmos.DrawCube(transform.position, box.size);
        }
    }
}
