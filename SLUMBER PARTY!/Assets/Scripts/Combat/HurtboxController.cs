using Environment;
using System.Globalization;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UIElements;
using static UnityEngine.Rendering.DebugUI;

namespace Combat
{
    public class HurtboxController : MonoBehaviour
    {
        HitboxController hitbox;
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

        private void OnTriggerEnter2D(Collider2D collision)
        {
            hitbox = collision.GetComponent<HitboxController>();
            if (hitbox == null) return;
            if (hitbox.owner == owner) return; // don't hit yourself!!!

            Debug.Log($"Trigger on {owner.OwnerClientId} | isServer={owner.IsServer} isOwner={owner.IsOwner}");

            HitResult result = new HitResult
            {
                damage = hitbox.data.damage,
                hitstun = hitbox.data.hitstunDuration,
                knockbackForce = hitbox.data.knockbackForce,
                direction = hitbox.data.direction.normalized,
                attackerFacing = hitbox.owner.facing,
                hitstop = hitbox.data.hitstopDuration
            };

            ReportHitServerRpc(
                owner.OwnerClientId,
                result
            );
        }

        [Rpc(SendTo.Server)]
        void ReportHitServerRpc(ulong attackerID, HitResult result)
        {
            //Debug.Log($"Sending hit to server: {attackerID}");
            if (!NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(
                attackerID, out var obj))
                return;

            setAndPlayAudio(hitbox.data.audio);
            owner.OnHit(owner.NetworkObjectId, result);
            FindAnyObjectByType<Hitstop>().Stop(hitbox.data.hitstopDuration);
        }

        private void setAndPlayAudio (AudioClip hitClip)
        {
            owner.audioSource.clip = hitClip;
            owner.audioSource.Play();
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
