using UnityEngine;
using System.Collections.Generic;
using TMPro;
using System.Linq;
using Unity.Netcode;

public class DropDownChooseTeam : MonoBehaviour
{
    private TMP_Dropdown dropdown;
    private List<PlayerInfos> oldList;

    private void Start()
    {
        dropdown = GetComponent<TMP_Dropdown>();
        if (dropdown == null) return;
        if (PlayerTable.Instance == null) return;
        oldList = new List<PlayerInfos>();
        dropdown.onValueChanged.AddListener(OnDropdownChanged);
        if (!NetworkManager.Singleton.IsServer) dropdown.interactable = false;
    }

    private void FixedUpdate()
    {
        if (dropdown == null)
        {
            dropdown = GetComponent<TMP_Dropdown>();
            return;
        }
        if (PlayerTable.Instance == null) return;
        if (!oldList.SequenceEqual(PlayerTable.Instance.playersAndBots))
        {
            dropdown.onValueChanged.RemoveAllListeners();
            dropdown.onValueChanged.AddListener(OnDropdownChanged);

            dropdown.ClearOptions();
            List<string> options = new List<string>();

            for (int i = 0; i < PlayerTable.Instance.playersAndBots.Count; i++) options.Add($"Team {i}");
            dropdown.AddOptions(options);
            dropdown.value = MapManager.Instance.playerList[transform.parent.GetSiblingIndex()].playerTeam;

            oldList = new List<PlayerInfos>(PlayerTable.Instance.playersAndBots); // Copie profonde
        }
    }

    private void OnDropdownChanged(int index)
    {
        if (!NetworkManager.Singleton.IsServer) return;
        string selectedOption = dropdown.options[index].text;

        // Tu peux parser l’index si tu veux retrouver le numéro de team
        if (selectedOption.StartsWith("Team "))
        {
            if (int.TryParse(selectedOption.Substring(5), out int teamNumber))
            {
                PlayerData _playerDatasForDropDownBox = MapManager.Instance.playerList[transform.parent.GetSiblingIndex()];
                MapManager.Instance.UpdatePlayerInfosServerRpc(transform.parent.GetSiblingIndex(),
                                                                _playerDatasForDropDownBox.playerName.ToString(),
                                                                _playerDatasForDropDownBox.playerColor,
                                                                teamNumber,
                                                                _playerDatasForDropDownBox.isBot);
            }
        }
    }
}