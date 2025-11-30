using UnityEngine;

[CreateAssetMenu(menuName = "Fighter/CharacterData")]
public class HitboxData : ScriptableObject
{
    public Vector2 offset;
    public Vector2 size;

    public float damage;
    public float knockBack;
    public float knockBackSpeed;
    public float knockBackAngle;
}
