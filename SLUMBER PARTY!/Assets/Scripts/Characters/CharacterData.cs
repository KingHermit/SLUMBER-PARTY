using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Fighter/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Technical")]
    [SerializeField] private int id;

    [Header("Name & Description")]
    public string CharacterName;
    public string CharacterDescription;
    public string CharacterType;

    [Header("Images & Whatnot")]
    public Sprite selectIcon;

    [Header("Base Stats")]
    public float speed;
    public float jumpForce;
    private float dmgPercent;

    [Header("Components")]
    public RuntimeAnimatorController animatorController;
    public List<MoveData> moves;

    public int Id => id;
    public string CharName => CharacterName;
    public string CharDesc => CharacterDescription;
    public string CharType => CharacterType;
    public Sprite icon => selectIcon;
    public float Speed => speed;
    public float JumpForce => jumpForce;
    public float DamagePercent => dmgPercent;
    public RuntimeAnimatorController AnimatorController => animatorController;
}
