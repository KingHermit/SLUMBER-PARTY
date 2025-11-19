using UnityEngine;

[CreateAssetMenu(menuName = "Fighter/CharacterData")]
public class CharacterData : ScriptableObject
{
    public string CharacterName;
    public string CharacterDescription;
    public string CharacterType;

    public float speed;
    public float jumpForce;
    public int maxHealth;

    public RuntimeAnimatorController animatorController;
}
