using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class DropDownChooseTeam : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private List<PlayerInfos> oldList;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown == null) return;
        if (PlayerTable.Instance == null) return;
        oldList = PlayerTable.Instance.playersAndBots;
    }

    private void FixedUpdate()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
            return;
        }
        if (PlayerTable.Instance == null) return;

        // if (oldList.Select(p => p.Name).SequenceEqual(PlayerTable.Instance.playersAndBots.Select(p => p.Name))) return;
        // Ajouter des options dynamiquement
        dropdown.ClearOptions();
        List<string> options = new List<string>();
        foreach (PlayerInfos player in PlayerTable.Instance.playersAndBots)
        {
            if (!options.Contains($"Team {player.Team}")) options.Add($"Team {player.Team}");
        }
        dropdown.AddOptions(options);
    }
}