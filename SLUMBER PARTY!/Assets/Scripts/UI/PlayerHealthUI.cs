using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class PlayerHealthUI : MonoBehaviour
{
    public TMP_Text dmg_Text;

    public void UpdateHealth(float value)
    {
        dmg_Text.text = value.ToString();
    }
}