using NUnit.Framework;
using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Fighter/CharacterData")]
public class CharacterData : ScriptableObject
{
    [Header("Name & Description")]
    public string CharacterName;
    public string CharacterDescription;
    public string CharacterType;

    [Header("Base Stats")]
    public float speed;
    public float jumpForce;
    public float maxHealth;

    [Header("Components")]
    public RuntimeAnimatorController animatorController;
    public List<MoveData> moves;
}
