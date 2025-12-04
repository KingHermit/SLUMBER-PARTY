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

    [Header("Damage")]
    public float damage;

    [Header("Knockback")]
    public float knockBack;
    public float knockBackSpeed;
    public float knockBackAngle;
}
