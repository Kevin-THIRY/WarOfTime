// TurnManager.cs
using System.Collections.Generic;
using Unity.Netcode;
using UnityEngine;
using UnityEngine.UI;

public class TurnManager : NetworkBehaviour
{
    private List<ulong> playerIds = new();
    private NetworkVariable<ulong> activePlayerId = new();
    private int currentTurnIndex = 0;

    public override void OnNetworkSpawn()
    {
        if (IsServer)
        {
            foreach (var client in NetworkManager.Singleton.ConnectedClientsList)
                playerIds.Add(client.ClientId);

            activePlayerId.Value = playerIds[0];
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void EndTurnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (rpcParams.Receive.SenderClientId != activePlayerId.Value)
        {
            Debug.LogWarning("Client a tenté de finir un tour qui n est pas le sien !");
            return;
        }

        currentTurnIndex = (currentTurnIndex + 1) % playerIds.Count;
        activePlayerId.Value = playerIds[currentTurnIndex];
    }

    public bool IsMyTurn()
    {
        return NetworkManager.Singleton.LocalClientId == activePlayerId.Value;
    }

    // void OnGUI()
    // {
    //     if (IsMyTurn())
    //     {
    //         GUI.Label(new Rect(10, 10, 300, 20), "C est ton tour !");
    //     }
    //     else
    //     {
    //         GUI.Label(new Rect(10, 10, 300, 20), "Attends ton tour...");
    //     }
    // }
} 
