using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMovementManager : MonoBehaviour
{
    private TerrainGenerator.GridCell[,] gridCells;

    void Start()
    {
        if (gridCells == null) Debug.LogWarning("gridCells not defined");
    }

    // void Update()
    // {
    //     navMeshAgent.destination = destination;
    // }

    // public void Goto()
    // {
    //     navMeshAgent.destination = destination;
    // }

    #region Setter
    public void SetGridCells(TerrainGenerator.GridCell[,] terrainGridCells) { gridCells = terrainGridCells; }
    #endregion
}