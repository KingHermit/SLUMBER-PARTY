using UnityEngine;

[CreateAssetMenu(menuName = "Fighter/MoveData")]
public class MoveData : ScriptableObject
{
    public string moveName;
    public float startup;
    public float active;
    public float recovery;

    public HitboxData hitbox;
}
