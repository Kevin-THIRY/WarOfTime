using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using System;

public static class UnitlList
{
    public static List<Unit> AllUnits = new List<Unit>();
}

public class Unit
{
    GameObject me;
    [NonSerialized] public bool isMoving = false;
    [NonSerialized] public int id;
    [NonSerialized] public string unitName;
    [NonSerialized] public Vector2 gridPosition;
    public Unit(GameObject me, int id, string unitName, Vector2 position)
    {
        this.me = me;
        this.id = id;
        this.unitName = unitName;
        this.gridPosition = position;
    }

    public IEnumerator Goto(List<Vector2> path, float speed, System.Action<bool> onComplete)
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
                
                while (Vector3.Distance(me.transform.position, targetWorldPos) > 0.1f)
                {
                    // Vector3 move = Vector3.MoveTowards(me.transform.position, targetWorldPos, speed * Time.deltaTime);
                    // me.GetComponent<Rigidbody>().MovePosition(move); // synchrone avec NetworkTransform
                    // yield return null;
                    me.transform.position = Vector3.MoveTowards(me.transform.position, targetWorldPos, speed * Time.deltaTime);
                    yield return null;
                }
                
                gridPosition = targetGridPos; // Met à jour la position une fois arrivé
            }
            isMoving = false;
            onComplete?.Invoke(true); // Succès
        }
    }
}

public class UnitManager : NetworkBehaviour
{
    private Unit selfUnit;
    // private Queue<Vector2> pathQueue;

    void Start()
    {
        (int x, int y) = ElementaryBasics.GetGridPositionFromWorldPosition(gameObject.transform.position);
        selfUnit = new Unit(gameObject, 0, gameObject.name, new Vector2(x, y));
        FindAnyObjectByType<PlayerManager>().SetSelectedUnit(selfUnit);
    }
}