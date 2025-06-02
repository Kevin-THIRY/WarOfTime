using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using System;
using System.Linq;

public enum UnitType
{
    MapManager,
    Peasant,
    Tank,
    Medic,
    HDV,
    // etc.
}

public enum NationType { France, England, Germany, Russia }

[Serializable]
public struct UnitMapping
{
    public UnitType unitType;
    public GameObject prefab;
}

public static class UnitList
{
    public static List<Unit> AllUnits = new List<Unit>();
    public static List<Unit> MyUnitsList = new List<Unit>();
}

public class Unit : NetworkBehaviour
{
    [NonSerialized] public bool isMoving = false;
    [NonSerialized] public int id;
    [NonSerialized] public int visibility = 2;
    [NonSerialized] public string unitName;
    [NonSerialized] public Vector2 gridPosition;
    [NonSerialized] public bool moveEnded;
    public bool isBuilding = false;
    public bool isBot; // ta variable locale bool
    public int botId = -1;

    void Start()
    {
        // var (x, y) = ElementaryBasics.GetGridPositionFromWorldPosition(transform.position);
        // id = UnitList.AllUnits.Count;
        // unitName = name;
        // gridPosition = new Vector2(x, y);
        // if (IsOwner) PlayerManager.instance.SetSelectedUnit(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        var (x, y) = ElementaryBasics.GetGridPositionFromWorldPosition(transform.position);
        id = UnitList.AllUnits.Count;
        unitName = name;

        gridPosition.x = x;
        gridPosition.y = y;

        if (IsOwner)
        {
            gameObject.layer = LayerMask.NameToLayer("HiddenFromPlayer");
            UnitList.MyUnitsList.Add(this);
            UpdateAllUnitListServerRpc(NetworkObjectId);
            var spawnCell = TerrainGenerator.instance.gridCells[x, y];
            spawnCell.isOccupied = true;
            MapManager.Instance.RequestGridCellUpdate(spawnCell);
            if (isBot) ChangeIntoBotServerRpc(NetworkObjectId, botId);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    public void UpdateAllUnitListServerRpc(ulong objectId, ServerRpcParams rpcParams = default)
    {
        if (!IsServer) return;
        UpdateAllUnitListClientRpc(objectId);
    }

    [ClientRpc]
    public void UpdateAllUnitListClientRpc(ulong objectId)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(objectId, out var netObj))
        {
            Unit unit = netObj.GetComponent<Unit>();
            UnitList.AllUnits.Add(unit);
        }
    }

    [ServerRpc(RequireOwnership = false)]
    void ChangeIntoBotServerRpc(ulong networkObjectId, int botId)
    {
        ChangeIntoBotClientRpc(networkObjectId, botId);
    }

    [ClientRpc]
    void ChangeIntoBotClientRpc(ulong networkObjectId, int botId)
    {
        // Trouve ton unité par NetworkObjectId
        var unit = NetworkManager.Singleton.SpawnManager.SpawnedObjects[networkObjectId].GetComponent<Unit>();
        if (unit == null) return;
        unit.isBot = true;
        unit.botId = botId;
        MoveUnitToBotList(unit, botId);
    }

    [ServerRpc(RequireOwnership = false)]
    public void NotifyUnitMovedServerRpc(ulong unitId, Vector2 newGridPos)
    {
        UpdateUnitPositionClientRpc(unitId, newGridPos);
    }

    [ClientRpc]
    private void UpdateUnitPositionClientRpc(ulong unitId, Vector2 newGridPos)
    {
        if (NetworkManager.Singleton.SpawnManager.SpawnedObjects.TryGetValue(unitId, out var netObj))
        {
            var unit = netObj.GetComponent<Unit>();
            unit.gridPosition = newGridPos;
        }
    }

    public bool IsOnABuilding()
    {
        var unitsOnCell = UnitList.MyUnitsList
            .Where(u => u.gridPosition == this.gridPosition)
            .ToList();

        if (unitsOnCell.Count > 0)
        {
            // Vérifie si l'une des unités sur la case est un bâtiment
            return unitsOnCell.Any(u => u.isBuilding);
        }

        return false;
    }

    public IEnumerator Goto(List<Vector2> path, float speed, System.Action<bool, Vector2> onComplete)
    {
        if (IsOwner)
        {
            if (path == null || path.Count == 0)
            {
                onComplete?.Invoke(false, Vector2.zero);
                yield return null;
            }
            if (!isMoving)
            {
                isMoving = true;
                foreach (Vector2 targetGridPos in path)
                {
                    Vector3 targetWorldPos = ElementaryBasics.GetWorldPositionFromGridCoordinates((int)targetGridPos.x, (int)targetGridPos.y, true);

                    while (Vector3.Distance(transform.position, targetWorldPos) > 0.1f)
                    {
                        transform.position = Vector3.MoveTowards(transform.position, targetWorldPos, speed * Time.deltaTime);
                        yield return null;
                    }

                    gridPosition = targetGridPos; // Met à jour la position une fois arrivé
                    NotifyUnitMovedServerRpc(NetworkObjectId, gridPosition);
                }
                isMoving = false;
                onComplete?.Invoke(true, gridPosition); // Succès
            }
        }
        else
        {
            onComplete?.Invoke(false, Vector2.zero);
            yield return null;
        }
    }
    
    public void MoveUnitToBotList(Unit unit, int botId)
    {
        // Enlève l'unité de MyUnitsList si elle y est
        UnitList.MyUnitsList.Remove(unit);

        // Cherche le BotOption correspondant au botId
        BotOption bot = BotList.Bots.Find(b => b.id == botId);

        if (bot == null)
        {
            Debug.LogWarning($"Bot avec id {botId} introuvable dans BotList !");
            return;
        }

        // Ajoute l'unité à la liste BotUnits de ce bot
        if (!bot.BotUnits.Contains(unit)) bot.BotUnits.Add(unit);
    }
}