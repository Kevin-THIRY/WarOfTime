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
            if (!IsHost) MapManager.Instance.AddPlayerServerRpc(GameData.playerInfos.Name, GameData.playerInfos.Color, GameData.playerInfos.Team, false);
            MovementManager.instance.SetInOutInventory(true);
            // RequestSpawnUnitServerRpc(nationType, UnitType.Peasant, Vector3.zero);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void InitSpawnServerRpc(ServerRpcParams rpcParams = default)
    {
        if (IsServer)
        {
            InitSpawnClientRpc();
            RequestSpawnUnitServerRpc(nationType, UnitType.Peasant, Vector3.zero); // Faire une fonction pour faire spawn les untités de départ d'un joueur

            for (int i = 0; i < AIManager.instance.idList.Count; i++)
            {
                AddBotServerRpc(PlayerTable.Instance.bots[i].botDifficulty, AIManager.instance.idList[i]);
                RequestSpawnUnitServerRpc(nationType, UnitType.Peasant, Vector3.zero, true, AIManager.instance.idList[i]);
                // SpawnBot(bot); // Fonction qui gere la meta du bot
                // SpawnBotUnits(bot); // Fonction qui fait spawn les unités de départ d'un bot
            }
        }
    }

    [ClientRpc]
    private void InitSpawnClientRpc()
    {
        if (!IsHost)
        {
            RequestSpawnUnitServerRpc(nationType, UnitType.Peasant, Vector3.zero); // Faire une fonction pour faire spawn les untités de départ d'un joueur
            MovementManager.instance.SetInOutInventory(false);
            MenuController.instance.SetBlockingCanvas(false);
            MenuController.instance.ChangePanel(Type.BaseUID, 0, type => type == Type.None, (type, button) => button.GetButtonType() == type);
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
    public void RequestSpawnUnitServerRpc(NationType nation, UnitType unitType, Vector3 spawnPos, bool isBot = false, int botId = -1, ServerRpcParams rpcParams = default)
    {
        ulong clientId = rpcParams.Receive.SenderClientId;

        GameObject prefab = GetPrefab(nation, unitType); // comme défini plus tôt
        if (prefab == null) return;

        GameObject unit = Instantiate(prefab, spawnPos, Quaternion.identity);
        
        Unit unitComponent = unit.GetComponent<Unit>(); // ton script avec IsBot
        if (unitComponent != null)
        {
            unitComponent.isBot = isBot;
            unitComponent.botId = botId;
        }
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

    [ServerRpc(RequireOwnership = false)]
    void AddBotServerRpc(BotDifficulty botDifficulty, int botId)
    {
        AddBotClientRpc(botDifficulty, botId);
    }

    [ClientRpc]
    void AddBotClientRpc(BotDifficulty botDifficulty, int botId)
    {
        BotOption bot = new BotOption();
        bot.botDifficulty = botDifficulty;
        bot.id = botId;
        BotList.Bots.Add(bot);
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