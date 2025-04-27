using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIMovementManager : MonoBehaviour
{
    void Start()
    {
        if (TerrainGenerator.instance.gridCells == null) Debug.LogWarning("gridCells not defined");
    }

    // void Update()
    // {
    //     navMeshAgent.destination = destination;
    // }

    // public void Goto()
    // {
    //     navMeshAgent.destination = destination;
    // }
}