using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkSpawnerManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> unitPrefabs; // Liste des prefabs dispo

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
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
}