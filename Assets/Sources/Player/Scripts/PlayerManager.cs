using System.Linq;
using UnityEngine;
using System.Collections.Generic;

public class Unit
{
    public int id;
    public string name;
    public Vector2 gridPosition;
    public Unit(int id, string name, Vector2 position)
    {
        this.id = id;
        this.name = name;
        this.gridPosition = position;
    }
}

public class PlayerManager : MonoBehaviour
{
    [SerializeField] private MouseShaderController mouseShaderController;
    [SerializeField] private GameObject firstUnit;
    [SerializeField] private TerrainGenerator terrainGenerator;
    private int id;
    private TerrainGenerator.GridCell[,] gridCells;
    private TerrainGenerator.GridCell selectedCell;
    private Unit selectedUnit;
    private List<Unit> allUnitsOfThePlayer = new List<Unit>();

    void Start()
    {
        id = 0;
        gridCells = terrainGenerator.GetGridCells();
        mouseShaderController.SetPlayerManager(this);
        (int x, int y) = GetCellFromWorldPosition(firstUnit.transform.position);
        allUnitsOfThePlayer.Add(new Unit(id++, firstUnit.name, new Vector2(x, y)));
    }

    void Update()
    {
        if (allUnitsOfThePlayer == null) return;

        foreach (Unit unit in allUnitsOfThePlayer)
        {
            Debug.Log(unit.name);
            Debug.Log(unit.gridPosition);
        }
    }

    public (int, int) GetCellFromWorldPosition(Vector3 worldPosition)
    {
        int width = gridCells.GetLength(0) - 1;
        int height = gridCells.GetLength(1) - 1;

        // Ajuste la taille des cellules pour que ça match pile avec la map
        float adjustedCellSize = width / (float)Mathf.RoundToInt(width / terrainGenerator.GetCellSize());

        // Convertit la position Unity en coordonnées de la grille
        int X = Mathf.Clamp(Mathf.RoundToInt(worldPosition.x / adjustedCellSize), 0, width - 1);
        int Y = Mathf.Clamp(Mathf.RoundToInt(worldPosition.z / adjustedCellSize), 0, height - 1);

        return (X, Y);
    }

    #region Setter
    public void SetSelectedCell(TerrainGenerator.GridCell _cell) { selectedCell = _cell; }
    public void AddUnit(Unit _unit) { allUnitsOfThePlayer.Append(_unit); }
    #endregion
}