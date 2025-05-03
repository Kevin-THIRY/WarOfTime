using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using System;

public class ElementaryBasics
{
    public static HashSet<(int, int)> revealedCells = new HashSet<(int, int)>();
    public static HashSet<(int, int)> visibleCells = new HashSet<(int, int)>();
    public static TerrainGenerator terrainGenerator { get; set;}
    public static (int, int) GetGridPositionFromWorldPosition(Vector3 worldPosition)
    {
        int gridX = TerrainGenerator.instance.gridCells.GetLength(0) - 1;
        int gridY = TerrainGenerator.instance.gridCells.GetLength(1) - 1;

        float adjustedCellSizeX = terrainGenerator.GetWidth() / gridX;
        float adjustedCellSizeY = terrainGenerator.GetWidth() / gridY;

        float x_position = worldPosition.x / adjustedCellSizeX;
        float z_position = worldPosition.z / adjustedCellSizeY;

        // Convertit la position Unity en coordonnées de la grille
        int X = Mathf.FloorToInt(Mathf.Clamp(x_position, 0, gridX - 1));
        int Y = Mathf.FloorToInt(Mathf.Clamp(z_position, 0, gridY - 1));

        return (X, Y);
    }

    public static Vector3 GetWorldPositionFromGridCoordinates(int x, int y, bool getCenterPosition = false)
    {
        float worldPosY = TerrainGenerator.instance.gridCells[x, y].position.y;

        if (getCenterPosition) 
        {
            float worldPosX = TerrainGenerator.instance.gridCells[x, y].center.x;
            float worldPosZ = TerrainGenerator.instance.gridCells[x, y].center.y;
            return new Vector3(worldPosX, worldPosY, worldPosZ);
        }
        else
        {
            float worldPosX = TerrainGenerator.instance.gridCells[x, y].position.x;
            float worldPosZ = TerrainGenerator.instance.gridCells[x, y].position.z;
            return new Vector3(worldPosX, worldPosY, worldPosZ);
        }
    }
}

public class PlayerManager : MonoBehaviour
{
    public static PlayerManager instance {private set; get;}
    [SerializeField] private TerrainGenerator terrainGenerator;
    [SerializeField] private LineRenderer lineRenderer;
    private TerrainGenerator.GridCell selectedCell;
    public Unit selectedUnit {private set; get;}
    private List<Unit> allUnitsOfThePlayer = new List<Unit>();
    private List<Vector2> path;

    private void Awake() {
        if(instance != null){
            Destroy(this);
            return;
        }

        instance = this;
    }

    void Start()
    {
    }

    void Update()
    {
    }

    public void MoveUnit()
    {
        if (TerrainGenerator.instance.gridCells == null || allUnitsOfThePlayer == null || selectedUnit == null || selectedCell == null) return;
        if (!selectedUnit.isMoving && selectedCell.gridPosition != selectedUnit.gridPosition)
        {
            path = FindPath(selectedUnit.gridPosition, selectedCell.gridPosition);
            ShowPathLine(path);
            TerrainGenerator.instance.gridCells[(int)selectedUnit.gridPosition.x, (int)selectedUnit.gridPosition.y].isOccupied = false;
            MapManager.Instance.RequestGridCellUpdate(TerrainGenerator.instance.gridCells[(int)selectedUnit.gridPosition.x, (int)selectedUnit.gridPosition.y]);
            StartCoroutine(selectedUnit.Goto(path, 10, (success, finalPosition) =>
            {
                if (success)
                {
                    var finalCell = TerrainGenerator.instance.gridCells[(int)finalPosition.x, (int)finalPosition.y];
                    finalCell.isOccupied = true;
                    MapManager.Instance.RequestGridCellUpdate(finalCell);

                    MovementManager.instance.SetInOutInventory(false);
                    selectedUnit = null;
                }
                else
                {
                    // Debug.Log("Échec du déplacement.");
                }
            }));
        }
    }

    public void Build()
    {
        if (TerrainGenerator.instance.gridCells == null || allUnitsOfThePlayer == null || selectedUnit == null || selectedCell == null) return;
        if (!selectedUnit.isMoving && selectedCell.gridPosition != selectedUnit.gridPosition)
        {
            selectedCell.isOccupied = true;
            MapManager.Instance.RequestGridCellUpdate(selectedCell);
            Vector3 posSpawn = ElementaryBasics.GetWorldPositionFromGridCoordinates((int)selectedCell.gridPosition.x, (int)selectedCell.gridPosition.y, true);
            NetworkSpawnerManager.Instance.RequestSpawnUnitServerRpc(NetworkSpawnerManager.Instance.nationType, UnitType.HDV, posSpawn);
            
            MovementManager.instance.SetInOutInventory(false);
        }
    }

    private void ShowPathLine(List<Vector2> path)
    {
        lineRenderer.positionCount = path.Count;
        for (int i = 0; i < path.Count; i++)
        {
            lineRenderer.SetPosition(i, ElementaryBasics.GetWorldPositionFromGridCoordinates((int)path[i].x, (int)path[i].y, true));
        }
    }

    private List<Vector2> FindPath(Vector2 start, Vector2 goal)
    {
        Dictionary<Vector2, float> costSoFar = new Dictionary<Vector2, float>();
        Dictionary<Vector2, Vector2> cameFrom = new Dictionary<Vector2, Vector2>();
        PriorityQueue<Vector2> frontier = new PriorityQueue<Vector2>();

        frontier.Enqueue(start, 0);
        costSoFar[start] = 0;

        int[] dx = { 0, 0, -1, 1 };
        int[] dy = { -1, 1, 0, 0 };

        while (frontier.Count > 0)
        {
            Vector2 current = frontier.Dequeue();

            if (current == goal) break;

            for (int i = 0; i < 4; i++)
            {
                Vector2 neighbor = new Vector2(current.x + dx[i], current.y + dy[i]);
                if (!IsInsideGrid((int)neighbor.x, (int)neighbor.y)) continue;

                // Check occupation ici
                // if (TerrainGenerator.instance.gridCells[(int)neighbor.x, (int)neighbor.y].isOccupied) continue;
                var cell = TerrainGenerator.instance.gridCells[(int)neighbor.x, (int)neighbor.y];

                bool isEnemyInvisible =
                    cell.isOccupied &&
                    UnitList.AllUnits.Any(u =>
                        u.gridPosition == neighbor &&
                        !UnitList.MyUnitsList.Contains(u) &&
                        !ElementaryBasics.visibleCells.Contains(((int)neighbor.x, (int)neighbor.y))
                    );

                if (cell.isOccupied && !isEnemyInvisible)
                    continue;
                

                float newCost = costSoFar[current] + TerrainGenerator.instance.gridCells[(int)neighbor.x, (int)neighbor.y].cost;

                if (!costSoFar.ContainsKey(neighbor) || newCost < costSoFar[neighbor])
                {
                    costSoFar[neighbor] = newCost;
                    cameFrom[neighbor] = current;
                    frontier.Enqueue(neighbor, newCost);
                }
            }
        }

        return ReconstructPath(cameFrom, start, goal);
    }

    private List<Vector2> ReconstructPath(Dictionary<Vector2, Vector2> cameFrom, Vector2 start, Vector2 goal)
    {
        List<Vector2> path = new List<Vector2>();

        if (!cameFrom.ContainsKey(goal))
        {
            // Peut-être que le goal est occupé => chercher la case la plus proche
            goal = FindNearestNonOccupied(cameFrom, goal);
            if (goal == Vector2.zero) return path; // Rien trouvé
        }

        Vector2 current = goal;
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        if (path.Count > 0)
        {
            Vector2 last = path.Last();
            if (UnitList.AllUnits.Any(u =>
                u.gridPosition == last &&
                !UnitList.MyUnitsList.Contains(u) &&
                !ElementaryBasics.visibleCells.Contains(((int)last.x, (int)last.y))
            ))
            {
                path.RemoveAt(path.Count - 1); // Stop avant
            }
        }
        return path;
    }

    private bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < TerrainGenerator.instance.gridCells.GetLength(0) && y >= 0 && y < TerrainGenerator.instance.gridCells.GetLength(1);
    }

    private Vector2 FindNearestNonOccupied(Dictionary<Vector2, Vector2> cameFrom, Vector2 goal)
    {
        Vector2 current = goal;

        while (cameFrom.ContainsKey(current))
        {
            int x = (int)current.x;
            int y = (int)current.y;
            if (!TerrainGenerator.instance.gridCells[x, y].isOccupied)
                return current;

            current = cameFrom[current];
        }

        return Vector2.zero; // Fail
    }

    #region Setter
    public void SetSelectedCell(TerrainGenerator.GridCell _cell) { selectedCell = _cell; }
    public void AddUnit(Unit _unit) { allUnitsOfThePlayer.Append(_unit); }
    public void SetSelectedUnit(Unit _unit) { selectedUnit = _unit; }
    #endregion
}

public class PriorityQueue<T>
{
    private SortedDictionary<float, Queue<T>> elements = new SortedDictionary<float, Queue<T>>();
    public int Count { get; private set; }
    
    public void Enqueue(T item, float priority)
    {
        if (!elements.ContainsKey(priority))
            elements[priority] = new Queue<T>();
        
        elements[priority].Enqueue(item);
        Count++;
    }

    public T Dequeue()
    {
        if (Count == 0) return default;
        
        var firstKey = elements.Keys.First();
        var queue = elements[firstKey];
        T item = queue.Dequeue();
        
        if (queue.Count == 0) elements.Remove(firstKey);
        Count--;
        return item;
    }
}