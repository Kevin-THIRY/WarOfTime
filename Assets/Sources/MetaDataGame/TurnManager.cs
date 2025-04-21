// TurnManager.cs
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    private List<ulong> playerIds = new();
    private NetworkVariable<ulong> activePlayerId = new();
    private int currentTurnIndex = 0;
    private Text turnText;
    private Text activePlayerIdText;
    private NetworkVariable<int> turnCount = new(0);

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            // playerIds = new List<ulong>();
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
            {
                playerIds.Add(client.ClientId);
            }
            activePlayerId.Value = playerIds[0];
        }

        if (IsOwner)
        {
            turnText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Nombre de tour").GetComponent<Text>();
            activePlayerIdText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Joueur actif ID").GetComponent<Text>();
            UpdateTurnDisplay();
            UpdateActivePlayerIdDisplay();
        }
    }

    [ClientRpc]
    public void UpdateTurnDisplayClientRpc()
    {
        UpdateTurnDisplay();
        UpdateActivePlayerIdDisplay();
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != activePlayerId.Value)
        {
            return;
        }
        Debug.Log(activePlayerId.Value);
        currentTurnIndex = (currentTurnIndex + 1) % playerIds.Count;
        foreach (var id in playerIds)
        {
            Debug.Log(id);
        }
        activePlayerId.Value = playerIds[currentTurnIndex];
        Debug.Log(activePlayerId.Value);
        turnCount.Value++;
        UpdateTurnDisplayClientRpc();
    }

    public void EndTurn()
    {
        if (IsOwner)
        {
            EndTurnServerRpc();
        }
    }


    private void UpdateTurnDisplay()
    {
        if (turnText != null)
            turnText.text = $"N o m b r e   d e   t o u r s   :   {turnCount.Value}";
    }

    private void UpdateActivePlayerIdDisplay()
    {
        if (activePlayerIdText != null)
            activePlayerIdText.text = $"I d   j o u e u r   a c t i f   :   {activePlayerId.Value}";
    }

    public bool IsMyTurn()
    {
        return NetworkManager.Singleton.LocalClientId == activePlayerId.Value;
    }
} 
