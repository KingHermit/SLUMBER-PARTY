using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class PlayerCard : MonoBehaviour
{
    [SerializeField] private CharacterDatabase characterDatabase;
    [SerializeField] private GameObject visuals;
    [SerializeField] private Image charIconImage;
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private TMP_Text charNameText;

    public void UpdateDisplay(CharacterSelectState state)
    {
        if (state.CharacterId != -1)
        {
            var character = characterDatabase.GetCharacterById(state.CharacterId);
            charIconImage.sprite = character.icon;
            charNameText.text = character.CharName;
            charIconImage.enabled = true;
        } else
        {
            charIconImage.enabled = false;
        }

        playerNameText.text = $"Player {state.ClientId}";

        visuals.SetActive(true);
    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }
}
