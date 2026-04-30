using UnityEngine;
using UnityEngine.UI;

public class CharacterSelectButton : MonoBehaviour
{
    [SerializeField] private Image iconImage;

    private CharacterSelectDisplay characterSelect;
    private CharacterData character;

    public void SetCharacter(CharacterSelectDisplay characterSelect, CharacterData character)
    {
        iconImage.sprite = character.icon;

        this.characterSelect = characterSelect;
        this.character = character;
    }

    public void SelectCharacter()
    {
        characterSelect.Select(character);
    }
}
