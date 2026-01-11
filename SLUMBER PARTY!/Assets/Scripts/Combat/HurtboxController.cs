using UnityEngine;
using Environment;
using UnityEngine.UIElements;

namespace Combat
{
    public class HurtboxController : MonoBehaviour
    {
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
            var hitbox = collision.GetComponent<HitboxController>();

            if (hitbox == null)
            {
                return;
            }

            if (hitbox.owner == owner) return; // don't hit yourself!!!

            setAndPlayAudio(hitbox.data.audio);
            owner.OnHit(hitbox);
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
