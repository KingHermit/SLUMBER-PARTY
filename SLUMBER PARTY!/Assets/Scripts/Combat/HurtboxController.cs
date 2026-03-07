using Environment;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using static UnityEngine.Rendering.DebugUI;

namespace Combat
{
    public class HurtboxController : NetworkBehaviour
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

        [Rpc(SendTo.Owner, InvokePermission = RpcInvokePermission.Everyone)]
        public void ReportHitServerRpc(MovePacketNet packet)
        {
            CharacterController attacker = GetNetworkObject(packet.attackerID).GetComponent<CharacterController>();

            HitboxData hbData = attacker.data.moves[packet.MoveIndex].hitboxes[packet.HitboxIndex];

            owner.ResolveHit(attacker, hbData, packet);
            PlayHitEffectsClientRpc(packet);
        }


        [Rpc(SendTo.Everyone)]
        private void PlayHitEffectsClientRpc(MovePacketNet packet)
        {
            CharacterController attacker = GetNetworkObject(packet.attackerID).GetComponent<CharacterController>();

            HitboxData hbData = attacker.data.moves[packet.MoveIndex].hitboxes[packet.HitboxIndex];

            PlayHitEffects(hbData);
        }

        private void PlayHitEffects(HitboxData hbData)
        {
            owner.audioSource.Play();
            FindAnyObjectByType<Hitstop>().Stop(hbData.hitstopDuration);
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
