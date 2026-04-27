using UnityEngine;

[CreateAssetMenu(fileName = "CharacterDatabase")]
public class CharacterDatabase : ScriptableObject
{
    public static CharacterDatabase instance;

    [SerializeField] private CharacterData[] characters = new CharacterData[0];

    public CharacterData[] GetAllCharacters() => characters;

    public CharacterData GetCharacterById(int id)
    {
        foreach (var character in characters)
        {
            if (character.Id == id)
            {
                return character;
            }
        }

        return null;
    }
}
