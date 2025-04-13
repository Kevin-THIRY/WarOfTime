using System.Linq;
using UnityEngine;
using System.Collections.Generic;
using System.Collections;
using Unity.Netcode;
using System;

public class ElementaryBasics
{
    public static TerrainGenerator terrainGenerator { get; set;}
    public static (int, int) GetGridPositionFromWorldPosition(Vector3 worldPosition)
    {
        int gridX = terrainGenerator.GetGridCells().GetLength(0) - 1;
        int gridY = terrainGenerator.GetGridCells().GetLength(1) - 1;

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
        TerrainGenerator.GridCell[,] gridCells = terrainGenerator.GetGridCells();
        float worldPosY = gridCells[x, y].position.y;

        if (getCenterPosition) 
        {
            float worldPosX = gridCells[x, y].center.x;
            float worldPosZ = gridCells[x, y].center.y;
            return new Vector3(worldPosX, worldPosY, worldPosZ);
        }
        else
        {
            float worldPosX = gridCells[x, y].position.x;
            float worldPosZ = gridCells[x, y].position.z;
            return new Vector3(worldPosX, worldPosY, worldPosZ);
        }
    }
}

public class Unit
{
    GameObject me;
    public bool isMoving = false;
    public int id;
    public string name;
    public Vector2 gridPosition;
    private Queue<Vector2> pathQueue;
    public Unit(GameObject me, int id, string name, Vector2 position)
    {
        this.me = me;
        this.id = id;
        this.name = name;
        this.gridPosition = position;
        this.pathQueue = new Queue<Vector2>();
    }

    public void Goto(List<Vector2> path)
    {
        if (path == null || path.Count == 0)
        {
            Debug.Log("Aucun chemin trouvé !");
            return;
        }
        
        pathQueue = new Queue<Vector2>(path);
        MoveToNextTile();
    }

    private void MoveToNextTile()
    {
        if (pathQueue.Count > 0)
        {
            gridPosition = pathQueue.Dequeue();
            Debug.Log($"Unité {name} déplacée en ({gridPosition.x}, {gridPosition.y})");
        }
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

public class PlayerManager : NetworkBehaviour
{
    [SerializeField] private GameObject firstUnit;
    [SerializeField] private TerrainGenerator terrainGenerator;
    [SerializeField] private LineRenderer lineRenderer;
    private MouseShaderController mouseShaderController;
    private int id;
    private TerrainGenerator.GridCell[,] gridCells;
    private TerrainGenerator.GridCell selectedCell;
    private Unit selectedUnit;
    private List<Unit> allUnitsOfThePlayer = new List<Unit>();
    private List<Vector2> path;

    void Start()
    {
        id = 0;
        gridCells = terrainGenerator.GetGridCells();
        mouseShaderController.SetPlayerManager(this);
        (int x, int y) = ElementaryBasics.GetGridPositionFromWorldPosition(firstUnit.transform.position);
        selectedUnit = new Unit(firstUnit, id++, firstUnit.name, new Vector2(x, y));
        allUnitsOfThePlayer.Add(selectedUnit);
    }

    void Update()
    {   
        if (!IsOwner) return;
        if (allUnitsOfThePlayer == null || selectedUnit == null || selectedCell == null) return;
        if (!selectedUnit.isMoving && selectedCell.gridPosition != selectedUnit.gridPosition)
        {
            path = FindPath(selectedUnit.gridPosition, selectedCell.gridPosition);
            ShowPathLine(path);
            // StartCoroutine(selectedUnit.Goto(path, 10));
            StartCoroutine(selectedUnit.Goto(path, 10, (success) =>
            {
                if (success)
                {
                    Debug.Log("Déplacement terminé avec succès !");
                }
                else
                {
                    Debug.Log("Échec du déplacement.");
                }
            }));
            // selectedUnit.Goto(path);
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
                
                float newCost = costSoFar[current] + gridCells[(int)neighbor.x, (int)neighbor.y].cost;
                
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
        if (!cameFrom.ContainsKey(goal)) return path; // Aucun chemin trouvé
        
        Vector2 current = goal;
        while (current != start)
        {
            path.Add(current);
            current = cameFrom[current];
        }
        path.Reverse();
        return path;
    }

    private bool IsInsideGrid(int x, int y)
    {
        return x >= 0 && x < gridCells.GetLength(0) && y >= 0 && y < gridCells.GetLength(1);
    }

    #region Setter
    public void SetSelectedCell(TerrainGenerator.GridCell _cell) { selectedCell = _cell; }
    public void SetTerrainGenerator(TerrainGenerator _terrainGenerator) { terrainGenerator = _terrainGenerator; }
    public void SetMouseShaderController(MouseShaderController _mouseShaderController) { mouseShaderController = _mouseShaderController; }
    public void AddUnit(Unit _unit) { allUnitsOfThePlayer.Append(_unit); }
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