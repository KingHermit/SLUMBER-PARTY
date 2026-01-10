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

    public HitboxData[] hitboxes;
}
