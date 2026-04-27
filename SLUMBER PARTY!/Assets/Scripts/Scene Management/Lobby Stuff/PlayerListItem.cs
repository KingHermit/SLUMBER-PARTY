using System;
using TMPro;
using Unity.Netcode;
using UnityEditor.PackageManager;
using UnityEngine;
using UnityEngine.TextCore.Text;
using WebSocketSharp;

public class PlayerListItem : MonoBehaviour
{
    [SerializeField] private TMP_Text playerNameText;
    [SerializeField] private GameObject visuals;

    public void UpdateDisplay(string playerName)
    {
        if (!playerName.IsNullOrEmpty())
        {
            playerNameText.text = playerName.ToString();
            visuals.SetActive(true);
        } else
        {
            DisableDisplay();
        }
    }

    public void DisableDisplay()
    {
        visuals.SetActive(false);
    }

}
