using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkSpawnerManager : NetworkBehaviour
{
    public static NetworkSpawnerManager Instance;
    [SerializeField] private List<GameObject> unitPrefabs; // Liste des prefabs dispo

    public override void OnNetworkSpawn()
    {

        // if (Instance != null && Instance != this)
        // {
        //     Destroy(gameObject);
        //     return;
        // }
        // Instance = this;

        // if (!IsServer)
        // {
        //     Destroy(gameObject); 
        // }

        if (IsOwner)
        {
            RequestAllUnitsServerRpc();
            if (IsServer) RequestSpawnUnitServerRpc(0, "MapManager");
            RequestSpawnUnitServerRpc(1);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnUnitServerRpc(int prefabIndex, string gameObjectName = "Unit", ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (prefabIndex < 0 || prefabIndex >= unitPrefabs.Count)
        {
            Debug.LogWarning($"Invalid prefab index {prefabIndex} from client {clientId}");
            return;
        }

        Vector3 spawnPos = new Vector3(0f, 0f, 0f);
        GameObject go = Instantiate(unitPrefabs[prefabIndex], spawnPos, Quaternion.identity);
        go.name = $"{gameObjectName}_{clientId}_P{prefabIndex}";
        go.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestAllUnitsServerRpc(ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        List<ulong> ids = new List<ulong>();
        foreach (var unit in UnitList.AllUnits)
        {
            ids.Add(unit.NetworkObjectId);
        }

        SendAllUnitsClientRpc(ids.ToArray(), clientId);
    }

    [ClientRpc]
    private void SendAllUnitsClientRpc(ulong[] unitIds, ulong targetClientId)
    {
        // S'assure que seul le bon client traite la réponse
        if (NetworkManager.Singleton.LocalClientId != targetClientId)
            return;

        foreach (var id in unitIds)
        {
            if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(id, out var netObj))
            {
                var unit = netObj.GetComponent<Unit>();
                UnitList.AllUnits.Add(unit);
            }
        }
    }
}