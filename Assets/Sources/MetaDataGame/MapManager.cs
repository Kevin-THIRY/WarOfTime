using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using Unity.Collections;

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance;
    private NetworkVariable<ulong> activePlayerId = new NetworkVariable<ulong>(0);
    private Text turnText;
    private Text activePlayerIdText;
    private Text playerNameText;
    private Text playerColorText;
    private Text playerTeamText;
    private NetworkVariable<int> turnCount = new NetworkVariable<int>(0);
    private NetworkVariable<FixedString64Bytes> playerName = new NetworkVariable<FixedString64Bytes>("");
    private NetworkVariable<Color> playerColor = new NetworkVariable<Color>(Color.white);
    private NetworkVariable<int> playerTeam = new NetworkVariable<int>(0);

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

        turnCount.OnValueChanged += UpdateTurnDisplay;
        activePlayerId.OnValueChanged += UpdateActivePlayerIdDisplay;

        playerName.OnValueChanged  += UpdatePlayerNameDisplay;
        playerColor.OnValueChanged += UpdatePlayerColorDisplay;
        playerTeam.OnValueChanged  += UpdateTeamDisplay;

        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
                activePlayerId.Value = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
            UpdatePlayerInfosServerRpc();
        }

        InitUIText();
    }

    private void InitUIText()
    {
        UpdateTurnDisplay(0, turnCount.Value);
        UpdateActivePlayerIdDisplay(0, activePlayerId.Value);
        UpdatePlayerNameDisplay("", playerName.Value);
        UpdatePlayerColorDisplay(Color.white, playerColor.Value);
        UpdateTeamDisplay(0, playerTeam.Value);
    }

    private void GetUITextInit()
    {
        turnText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Nombre de tour").GetComponent<Text>();
        activePlayerIdText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Joueur actif ID").GetComponent<Text>();

        playerNameText = GameObject.Find("MenuSwitch").transform.Find("MultiSettings").transform.Find("HostRowDisplay").transform.Find("PlayerName").GetComponent<Text>();
        playerColorText = GameObject.Find("MenuSwitch").transform.Find("MultiSettings").transform.Find("HostRowDisplay").transform.Find("PlayerColor").GetComponent<Text>();
        playerTeamText = GameObject.Find("MenuSwitch").transform.Find("MultiSettings").transform.Find("HostRowDisplay").transform.Find("PlayerTeam").GetComponent<Text>();
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

    private void UpdateTurnDisplay(int oldValue, int _turnCount)
    {
        if (turnText != null)
            turnText.text = $"N o m b r e   d e   t o u r s   :   {_turnCount}";
    }

    private void UpdateActivePlayerIdDisplay(ulong oldValue, ulong _activePlayerId)
    {
        if (activePlayerIdText != null)
            activePlayerIdText.text = $"I d   j o u e u r   a c t i f   :   {_activePlayerId}";
    }

    [ServerRpc(RequireOwnership = false)]
    private void UpdatePlayerInfosServerRpc(ServerRpcParams rpcParams = default)
    {
        playerName.Value  = GameData.playerInfos.Name;
        playerColor.Value = GameData.playerInfos.Color;
        playerTeam.Value  = GameData.playerInfos.Team;
    }

    private void UpdatePlayerNameDisplay(FixedString64Bytes oldValue, FixedString64Bytes _playerName)
    {
        if (playerNameText != null)
            playerNameText.text = $"{_playerName}";
    }

    private void UpdatePlayerColorDisplay(Color oldValue, Color _playerColor)
    {
        if (playerColorText != null)
            playerColorText.text = $"{_playerColor}";
    }

    private void UpdateTeamDisplay(int oldValue, int _playerTeam)
    {
        if (playerTeamText != null)
            playerTeamText.text = $"{_playerTeam}";
    }

    public bool IsMyTurn()
    {
        return NetworkManager.Singleton.LocalClientId == activePlayerId.Value;
    }
} 
