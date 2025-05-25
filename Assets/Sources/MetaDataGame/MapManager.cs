using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using Unity.Collections;
using System;


[System.Serializable]
public struct PlayerData : INetworkSerializable, IEquatable<PlayerData>
{
    public FixedString64Bytes playerName;
    public Color playerColor;
    public int playerTeam;
    public bool isBot;

    public void NetworkSerialize<T>(BufferSerializer<T> serializer) where T : IReaderWriter
    {
        serializer.SerializeValue(ref playerName);
        serializer.SerializeValue(ref playerColor);
        serializer.SerializeValue(ref playerTeam);
        serializer.SerializeValue(ref isBot);
    }

    // Implémentation de IEquatable pour NetworkList
    public bool Equals(PlayerData other)
    {
        return playerName.Equals(other.playerName) &&
               playerColor.Equals(other.playerColor) &&
               playerTeam == other.playerTeam &&
               isBot == other.isBot;
    }

    public override int GetHashCode()
    {
        return playerName.GetHashCode() ^ playerColor.GetHashCode() ^ playerTeam.GetHashCode() ^ isBot.GetHashCode();
    }
}

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance;
    
    private Text turnText;
    private Text activePlayerIdText;

    private NetworkVariable<int> turnCount = new NetworkVariable<int>(0);
    private NetworkVariable<ulong> activePlayerId = new NetworkVariable<ulong>(0);

    public NetworkList<PlayerData> playerList = new NetworkList<PlayerData>();

    public void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer && !IsOwner)
        {
            Destroy(gameObject);
        }
        else Instance = this;

        GetUITextInit();

        turnCount.OnValueChanged += (oldValue, newValue) => UpdateDisplay(newValue, turnText);
        activePlayerId.OnValueChanged += (oldValue, newValue) => UpdateDisplay(newValue, activePlayerIdText);
        
        playerList.OnListChanged += UpdatePlayerTable;

        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
                activePlayerId.Value = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
        }

        InitUIText();
        ResetPlayerTable();
        if (IsServer) AddPlayerServerRpc(GameData.playerInfos.Name, GameData.playerInfos.Color, GameData.playerInfos.Team, GameData.playerInfos.isBot);
    }

    private void ResetPlayerTable()
    {
        // Vider la table avant de la remplir
        PlayerTable.Instance.ClearTable();

        // Ajouter chaque joueur présent
        foreach (PlayerData player in playerList)
        {
            PlayerTable.Instance.AddPlayerRow(player.playerName.ToString(), player.playerColor, player.playerTeam, player.isBot);
        }
    }

    private void UpdatePlayerTable()
    {
        PlayerTable.Instance.ClearLists();
        // Ajouter chaque joueur présent
        for (int i = 0; i < playerList.Count; i++)
        {
            PlayerData player = playerList[i];
            PlayerTable.Instance.UpdatePlayerRow(i, player.playerName.ToString(), player.playerColor, player.playerTeam, player.isBot);
        }
    }

    private void UpdatePlayerTable(NetworkListEvent<PlayerData> changeEvent)
    {
        switch (changeEvent.Type)
        {
            case NetworkListEvent<PlayerData>.EventType.Add:
                // Ajouter la nouvelle ligne
                var newPlayer = changeEvent.Value;
                PlayerTable.Instance.AddPlayerRow(newPlayer.playerName.ToString(), newPlayer.playerColor, newPlayer.playerTeam, newPlayer.isBot);
                break;

            case NetworkListEvent<PlayerData>.EventType.Remove:
                // Supprimer la ligne correspondante
                var removedPlayer = changeEvent.Value;
                PlayerTable.Instance.RemovePlayerRow(removedPlayer.playerName.ToString());
                break;

            case NetworkListEvent<PlayerData>.EventType.Value:
                UpdatePlayerTable();
                break;
            case NetworkListEvent<PlayerData>.EventType.Insert:
            case NetworkListEvent<PlayerData>.EventType.Clear:
                // Rafraîchir complètement la table si l'événement est complexe
                ResetPlayerTable();
                break;
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void AddPlayerServerRpc(FixedString64Bytes name, Color color, int team, bool bot)
    {
        if (playerList.Count >= 4)
        {
            Debug.LogWarning("Impossible d'ajouter le joueur : nombre maximum atteint.");
            return;
        }
        PlayerData newPlayer = new PlayerData
        {
            playerName = name,
            playerColor = color,
            playerTeam = team,
            isBot = bot
        };
        playerList.Add(newPlayer);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerFromNameServerRpc(FixedString64Bytes name)
    {
        if (playerList == null || playerList.Count == 0) return;

        // Cherche l'index du joueur à supprimer
        int indexToRemove = -1;
        for (int i = 0; i < playerList.Count; i++)
        {
            if (playerList[i].playerName == name)
            {
                indexToRemove = i;
                break;
            }
        }

        if (indexToRemove != -1) playerList.RemoveAt(indexToRemove);
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdatePlayerInfosServerRpc(int idx, FixedString64Bytes name, Color color, int team, bool bot)
    {
        if (playerList == null || playerList.Count == 0) return;

        // Cherche l'index du joueur à supprimer
        playerList[idx] = new PlayerData { playerName = name, playerColor = color, playerTeam = team, isBot = bot };
    }

    [ServerRpc(RequireOwnership = false)]
    public void RemovePlayerFromRowIndexServerRpc(int index)
    {
        if (playerList == null || playerList.Count == 0) return;
        playerList.RemoveAt(index);
    }

    private void InitUIText()
    {
        UpdateDisplay(turnCount.Value, turnText);
        UpdateDisplay(activePlayerId.Value, activePlayerIdText);
    }

    private void GetUITextInit()
    {
        turnText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Nombre de tour").GetComponent<Text>();
        activePlayerIdText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Joueur actif ID").GetComponent<Text>();
    }

    public void RequestGridCellUpdate(TerrainGenerator.GridCell updatedCell)
    {
        // Client demande au serveur de mettre à jour une cellule
        UpdateGridCellServerRpc(
            updatedCell.gridPosition,
            updatedCell.isOccupied,
            updatedCell.resourceType
        );
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdateGridCellServerRpc(Vector2 gridPos, bool isOccupied, ResourcesType resourceType)
    {
        int x = (int)gridPos.x;
        int y = (int)gridPos.y;

        // Mettre à jour côté serveur
        TerrainGenerator.instance.gridCells[x, y].isOccupied = isOccupied;
        TerrainGenerator.instance.gridCells[x, y].resourceType = resourceType;
        Debug.Log($"Cellule : {gridPos} is occupied : {isOccupied}");

        // Propager aux autres clients
        UpdateGridCellClientRpc(gridPos, isOccupied, resourceType);
    }

    [ClientRpc]
    private void UpdateGridCellClientRpc(Vector2 gridPos, bool isOccupied, ResourcesType resourceType)
    {
        if (IsServer) return; // Le serveur a déjà mis à jour sa version

        int x = (int)gridPos.x;
        int y = (int)gridPos.y;

        Debug.Log($"Cellule : {gridPos} is occupied : {isOccupied}");

        // Mettre à jour localement sur les clients
        TerrainGenerator.instance.gridCells[x, y].isOccupied = isOccupied;
        TerrainGenerator.instance.gridCells[x, y].resourceType = resourceType;
    }

    public void EndTurn()
    {
        if (activePlayerId.Value == NetworkManager.Singleton.LocalClientId)
        {
            PlayerManager.instance.turnEnded = true;
            RequestEndTurnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestEndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;

        var clients = NetworkManager.Singleton.ConnectedClientsList;
        int currentIndex = -1;
        for (int i = 0; i < clients.Count; i++)
        {
            if (clients[i].ClientId == activePlayerId.Value)
            {
                currentIndex = i;
                break;
            }
        }

        if (currentIndex == -1) currentIndex = 0;

        int nextIndex = (currentIndex + 1) % clients.Count;
        activePlayerId.Value = clients[nextIndex].ClientId;

        turnCount.Value++;
    }

    private void UpdateDisplay<T>(T value, Text textComponent)
    {
        if (textComponent != null)
            textComponent.text = value.ToString();
    }

    public bool IsMyTurn()
    {
        return NetworkManager.Singleton.LocalClientId == activePlayerId.Value;
    }
} 
