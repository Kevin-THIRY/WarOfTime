using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;

public class NetworkSpawnerManager : NetworkBehaviour
{
    [SerializeField] private List<GameObject> unitPrefabs; // Liste des prefabs dispo
    [SerializeField] private int selectedPrefabIndex = 0; // Index choisi par le client

    private void Start()
    {
        
    }

    public override void OnNetworkSpawn()
    {
        if (IsOwner)
        {
            RequestSpawnUnitServerRpc(0);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnUnitServerRpc(int prefabIndex, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        if (prefabIndex < 0 || prefabIndex >= unitPrefabs.Count)
        {
            Debug.LogWarning($"Invalid prefab index {prefabIndex} from client {clientId}");
            return;
        }

        Vector3 spawnPos = new Vector3(0f, 0f, 0f);
        GameObject go = Instantiate(unitPrefabs[prefabIndex], spawnPos, Quaternion.identity);
        go.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        go.name = $"Unit_{clientId}_P{prefabIndex}";
    }

    #region Setter
    // Permet de choisir une prefab côté client
    public void SetSelectedPrefabIndex(int index)
    {
        selectedPrefabIndex = index;
    }
    #endregion
}