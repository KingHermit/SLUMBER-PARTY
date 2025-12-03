using UnityEngine;

[CreateAssetMenu(menuName = "Fighter/MoveData")]
public class MoveData : ScriptableObject
{
    public string moveName;
    public string type; // Light, Medium, Heavy, SPECIAL
    public int startup;
    public int active;
    public int recovery;

    public HitboxData[] hitboxes;
}
