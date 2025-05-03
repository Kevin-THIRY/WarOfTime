using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using System;

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

    void Start()
    {
        var (x, y) = ElementaryBasics.GetGridPositionFromWorldPosition(transform.position);
        id = UnitList.AllUnits.Count;
        unitName = name;
        gridPosition = new Vector2(x, y);
        // if (IsOwner) PlayerManager.instance.SetSelectedUnit(this);
    }

    public override void OnNetworkSpawn()
    {
        base.OnNetworkSpawn();

        if (IsOwner) 
        {
            gameObject.layer = LayerMask.NameToLayer("HiddenFromPlayer");
            UnitList.MyUnitsList.Add(this);
            UpdateAllUnitListServerRpc(NetworkObjectId);
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
}