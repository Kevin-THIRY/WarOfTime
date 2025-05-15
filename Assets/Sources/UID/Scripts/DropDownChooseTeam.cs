using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DropDownChooseTeam : MonoBehaviour
{
    [SerializeField] private TMP_Dropdown dropdown;

    private void Start()
    {
        if (dropdown == null) return;
        if (PlayerTable.Instance == null) return;

        // Ajouter des options dynamiquement
        dropdown.ClearOptions();
        PlayerTable.Instance.GetPlayerList();

        List<string> options = new List<string>();
        List<string> playerNames = PlayerTable.Instance.GetPlayerList();
        for (int i = 0; i < playerNames.Count; i++) options.Add($"Team {i + 1} - {playerNames[i]}");
        dropdown.AddOptions(options);
    }
}