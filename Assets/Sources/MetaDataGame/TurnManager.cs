// TurnManager.cs
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;

public class TurnManager : NetworkBehaviour
{
    private List<ulong> playerIds = new();
    private NetworkVariable<ulong> activePlayerId = new NetworkVariable<ulong>(0);
    private Text turnText;
    private Text activePlayerIdText;
    private NetworkVariable<int> turnCount = new NetworkVariable<int>(0);


    public void Start()
    {
        turnText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Nombre de tour").GetComponent<Text>();
        activePlayerIdText = GameObject.Find("MenuSwitch").transform.Find("Base UID Game").transform.Find("Joueur actif ID").GetComponent<Text>();
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        turnCount.OnValueChanged += UpdateTurnDisplay;
        activePlayerId.OnValueChanged += UpdateActivePlayerIdDisplay;

        if (IsServer)
        {
            if (playerIds.Count == 0) // Si la liste est vide, on la remplie
            {
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    playerIds.Add(client.ClientId);
                }
                activePlayerId.Value = playerIds[0];
            }
            else
            {
                // On ajoute le client qui vient de se connecter
                foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                {
                    if (!playerIds.Contains(client.ClientId)) // Si le client n'est pas déjà dans la liste
                    {
                        playerIds.Add(client.ClientId);
                    }
                }
            }
        }
    }

    public void EndTurn()
    {
        Debug.Log($"Demandande de fin de tour par le client : {NetworkManager.Singleton.LocalClientId}");
        Debug.Log(activePlayerId.Value);
        if (activePlayerId.Value == NetworkManager.Singleton.LocalClientId)
        {
            RequestEndTurnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestEndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            int currentTurnIndex = (playerIds.IndexOf(activePlayerId.Value) + 1) % playerIds.Count;
            activePlayerId.Value = playerIds[currentTurnIndex];
            turnCount.Value++;
        }
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
