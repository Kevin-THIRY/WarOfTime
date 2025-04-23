// TurnManager.cs
using System;
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine.UI;
using UnityEngine;
using Unity.VisualScripting;

public class TurnManager : NetworkBehaviour
{
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

        if (IsHost)
        {
            activePlayerId.Value = OwnerClientId;
        }
    }

    public void EndTurn()
    {
        Debug.Log($"Demandande de fin de tour par le client : {NetworkManager.Singleton.LocalClientId}");
        Debug.Log(activePlayerId.Value);
        if (IsOwner && activePlayerId.Value == NetworkManager.Singleton.LocalClientId)
        {
            RequestEndTurnServerRpc();
        }
    }

    [ServerRpc(RequireOwnership = false)]
    private void RequestEndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            activePlayerId.Value = rpcParams.Receive.SenderClientId++ % ((ulong)NetworkManager.Singleton.ConnectedClients.Count);
            turnCount.Value++;
        }
        else
        {
            Debug.Log("Je ne suis pas serveur et je fais le fou");
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
