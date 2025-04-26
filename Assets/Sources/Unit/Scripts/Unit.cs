using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using System;

public static class UnitlList
{
    public static List<Unit> AllUnits = new List<Unit>();
}

public class Unit : NetworkBehaviour
{
    [NonSerialized] public bool isMoving = false;
    [NonSerialized] public int id;
    [NonSerialized] public string unitName;
    [NonSerialized] public Vector2 gridPosition;

    void Start()
    {
        var (x, y) = ElementaryBasics.GetGridPositionFromWorldPosition(transform.position);
        id = 0;
        unitName = name;
        gridPosition = new Vector2(x, y);
        if (IsOwner) PlayerManager.instance.SetSelectedUnit(this);
    }

    public IEnumerator Goto(List<Vector2> path, float speed, System.Action<bool> onComplete)
    {
        if (IsOwner)
        {
            if (path == null || path.Count == 0) 
            {
                onComplete?.Invoke(false);
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
                }
                isMoving = false;
                onComplete?.Invoke(true); // Succès
            }
        }
        else
        {
            onComplete?.Invoke(false);
            yield return null;
        }
    }
}