using UnityEngine;

[CreateAssetMenu(menuName = "Fighter/HitboxData")]
public class HitboxData : ScriptableObject
{
    public GameObject HitboxPrefab;

    [Header("Size & Position")]
    public Vector2 offset;
    public Vector2 size;

    [Header("Duration")]
    public int startFrame; // Relative to start of MoveData.ACTIVE
    public int endFrame;   // Relative to start of MoveData.ACTIVE

    [Header("Hitstun/Hitstop")]
    [Range(0f, 3f)]
    public float hitstunDuration;
    [Range(0f, 0.75f)]
    public float hitstopDuration;

    [Header("Damage")]
    public float damage;

    [Header("Knockback")]
    public Vector2 direction;
    public float knockbackForce;

    [Header("Sound / VFX")]
    public AudioClip audio;
}
