/*
* Author: Zhi Hng
* Date: 24 July 2026
* Description: Handles the AI for all the NPCs.
*/

using UnityEngine;
using UnityEngine.AI;

public class NPCScript : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] string npcType;
    Vector3[] targetPositions = new Vector3[] 
    {   new Vector3(-5f, 0.5f, -5f),
        new Vector3(5f, 0.5f, -5f),
        new Vector3(-5f, 0.5f, 5f),
        new Vector3(5f, 0.5f, 5f)
    };
    Vector3 currentTargetPosition;
    //PickPocket Variables

    //Smoker Variables
    Vector3[] smokerTargetPositions = new Vector3[] 
    {   new Vector3(-5f, 0.5f, -5f),
        new Vector3(5f, 0.5f, -5f),
        new Vector3(-5f, 0.5f, 5f),
        new Vector3(5f, 0.5f, 5f)
    };
    //Fighter Variables
    Vector3[] fighterTargetPositions = new Vector3[] 
    {   new Vector3(-5f, 0.5f, -5f),
        new Vector3(5f, 0.5f, -5f),
        new Vector3(-5f, 0.5f, 5f),
        new Vector3(5f, 0.5f, 5f)
    };

    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (npcType == "Civilian")
        {
            currentTargetPosition = targetPositions[Random.Range(0, targetPositions.Length)];
        }
        else if (npcType == "Smoker")
        {
            currentTargetPosition = smokerTargetPositions[Random.Range(0, smokerTargetPositions.Length)];
        }
        else if (npcType == "Fighter")
        {
            currentTargetPosition = fighterTargetPositions[Random.Range(0, fighterTargetPositions.Length)];
        }
        
    }
    void Update()
    {
        if (npcType == "Civilian")
        {
            // Implement Civilian behavior here
        }
        else if (npcType == "PickPocket")
        {
            // Implement PickPocket behavior here
        }
        else if (npcType == "Smoker")
        {
            // Implement Smoker behavior here
        }
        else if (npcType == "Fighter")
        {
            // Implement Fighter behavior here
        }
    }
    void MoveToTargetPosition()
    {
        agent.SetDestination(currentTargetPosition);
    }
}