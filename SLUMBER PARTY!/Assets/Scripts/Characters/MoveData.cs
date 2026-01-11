using UnityEngine;

[CreateAssetMenu(menuName = "Fighter/MoveData")]
public class MoveData : ScriptableObject
{
    public string moveName;
    public string type; // Light, Medium, Heavy, SPECIAL

    [Header("Frames")]
    public int startup;
    public int active;
    public int recovery;

    [Header("Animation")]
    public AnimationClip animation;

    [Header("Hitboxes")]
    public HitboxData[] hitboxes;

    [Header("Etc.")]
    public bool canMove = true;
}
