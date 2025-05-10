using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

public class MapManager : NetworkBehaviour
{
    public static MapManager Instance;
    private NetworkVariable<ulong> activePlayerId = new NetworkVariable<ulong>(0);
    private Text turnText;
    private Text activePlayerIdText;
    private NetworkVariable<int> turnCount = new NetworkVariable<int>(0);

    // private void Awake()
    // {
    //     if (Instance != null && Instance != this)
    //     {
    //         Destroy(gameObject);
    //         return;
    //     }
    //     Instance = this;

    //     if (!IsServer)
    //     {
    //         Destroy(gameObject); 
    //     }
    // }

    public void Start()
    {
        turnText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Nombre de tour").GetComponent<Text>();
        activePlayerIdText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Joueur actif ID").GetComponent<Text>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsServer && !IsOwner)
        {
            Destroy(gameObject);
        }
        else Instance = this;

        turnCount.OnValueChanged += UpdateTurnDisplay;
        activePlayerId.OnValueChanged += UpdateActivePlayerIdDisplay;

        if (IsServer)
        {
            if (NetworkManager.Singleton.ConnectedClientsList.Count > 0)
                activePlayerId.Value = NetworkManager.Singleton.ConnectedClientsList[0].ClientId;
        }
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

    public bool IsMyTurn()
    {
        return NetworkManager.Singleton.LocalClientId == activePlayerId.Value;
    }
} 
