using UnityEngine;
using Unity.Netcode;
using UnityEngine.UI;
using System.Collections.Generic;
using System;

public class NetworkSpawnerManager : NetworkBehaviour
{
    public static NetworkSpawnerManager Instance;
    // [SerializeField] private List<GameObject> unitPrefabs; // Liste des prefabs dispo
    [SerializeField] private List<UnitMapping> unitMappings;
    private Dictionary<NationType, Dictionary<UnitType, GameObject>> nationUnitDict;
    [SerializeField] public NationType nationType;

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();
        
        nationUnitDict = new();
        nationUnitDict[nationType] = new();

        foreach (var unit in unitMappings)
        {
            nationUnitDict[nationType][unit.unitType] = unit.prefab;
        }

        if (IsOwner)
        {
            Instance = this;
            RequestAllUnitsServerRpc();
            if (IsServer) RequestSpawnUnitServerRpc(nationType, UnitType.MapManager, Vector3.zero);
            MovementManager.instance.SetInOutInventory(true);
            // RequestSpawnUnitServerRpc(nationType, UnitType.Peasant, Vector3.zero);
        }
    }

    public GameObject GetPrefab(NationType nation, UnitType unit)
    {
        if (nationUnitDict.TryGetValue(nation, out var unitDict) &&
            unitDict.TryGetValue(unit, out var prefab))
        {
            return prefab;
        }

        Debug.LogWarning($"Aucun prefab trouvé pour {nation} - {unit}");
        return null;
    }

    [ServerRpc(RequireOwnership = false)]
    public void RequestSpawnUnitServerRpc(NationType nation, UnitType unitType, Vector3 spawnPos, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        GameObject prefab = GetPrefab(nation, unitType); // comme défini plus tôt
        if (prefab == null) return;

        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity);
        
        unit.name = $"Client_{clientId}_Nation_{nation}_Unit_{unitType}";
        unit.GetComponent<NetworkObject>().SpawnWithOwnership(clientId);
        SetLayerRecursively(unit, LayerMask.NameToLayer("VisibleToPlayer"));
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
                TerrainGenerator.instance.gridCells[(int)unit.gridPosition.x, (int)unit.gridPosition.y].isOccupied = true;
            }
        }
    }

    private void SetLayerRecursively(GameObject obj, int newLayer)
    {
        obj.layer = newLayer;

        foreach (Transform child in obj.transform)
        {
            SetLayerRecursively(child.gameObject, newLayer);
        }
    }
}