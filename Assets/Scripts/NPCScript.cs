/*
* Author: Zhi Hng
* Date: 25 July 2026
* Description: Handles the AI for all the NPCs.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCScript : MonoBehaviour
{
    NavMeshAgent agent;
    [SerializeField] string npcType;
    Vector3 currentTargetPosition;
    Coroutine timerBeforeDestroyCoroutine;
    //PickPocket Variables
    GameObject targetCivilian;

    //Smoker Variables


    //Fighter Variables


    void Start()
    {
        agent = GetComponent<NavMeshAgent>();
        if (npcType == "Civilian")
        {
            while (true)
            {
                currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
                if (transform.position != currentTargetPosition)
                {
                    break; // Exit the loop if the target position is valid
                }
            }
            MoveToTargetPosition();
        }
        else if (npcType == "PickPocket")
        {
            targetCivilian = NPCManager.spawnedCivilians[Random.Range(0, NPCManager.spawnedCivilians.Count)];
        }
        else if (npcType == "Smoker")
        {
            while (true)
            {
                currentTargetPosition = NPCManager.eventPoints[Random.Range(0, NPCManager.eventPoints.Length)].position;
                if (transform.position != currentTargetPosition)
                {
                    break; // Exit the loop if the target position is valid
                }
            }
            MoveToTargetPosition();
        }
        else if (npcType == "Fighter")
        {
            while (true)
            {
                currentTargetPosition = NPCManager.eventPoints[Random.Range(0, NPCManager.eventPoints.Length)].position;
                if (transform.position != currentTargetPosition)
                {
                    break; // Exit the loop if the target position is valid
                }
            }
            MoveToTargetPosition();
        }
        
    }
    void Update()
    {
        if (npcType == "Civilian")
        {
            // Civilian behavior here
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
            {
                NPCManager.spawnedCivilians.Remove(gameObject); // Remove the civilian from the list of spawned civilians
                Destroy(gameObject); // Destroy the civilian when it reaches its target position
            }
        }
        else if (npcType == "Pickpocket")
        {
            // PickPocket behavior here
            if (targetCivilian != null)
            {
                currentTargetPosition = targetCivilian.transform.position;
                MoveToTargetPosition();
            }
            else
            {
                // If the target civilian is null (destroyed), pick a new target civilian
                if (NPCManager.spawnedCivilians.Count > 0)
                {
                    targetCivilian = NPCManager.spawnedCivilians[Random.Range(0, NPCManager.spawnedCivilians.Count)];
                    if (timerBeforeDestroyCoroutine != null)
                    {
                        StopCoroutine(timerBeforeDestroyCoroutine); // Stop the timer coroutine if it's running
                        timerBeforeDestroyCoroutine = null; // Reset the coroutine reference
                    }
                }
                else
                {
                    if (timerBeforeDestroyCoroutine == null)
                    {
                        timerBeforeDestroyCoroutine = StartCoroutine(TimerBeforeDestroy(5));
                    }
                }
            }
        }
        else if (npcType == "Smoker")
        {
            // Smoker behavior here
            // Yee Shen code here
            // Once reaching target position, check if player is far away, if yes, start smoking, if no, variable patience goes down for every second player is nearby.
            // If patience reaches 0, start smoking anyway. Smoker will turn around periodically and when 'field of view' detects player, starts running away from the player.
            // Run until far enough from the player for 5 seconds, then delete the smoker. If player interacts with smoker, add score and delete smoker.

        }
        else if (npcType == "Fighter")
        {
            // Fighter behavior here
            // Joel code here
            // Once reaching target position. wait until another fighter reaches the same position. Then start fighting with the cloud vfx overlayed.
            // Every second, variable severity increases.
            // After 20 sec if player does not interact, delete both fighters. If player interacts, add score. Add more score if severity is lower.

        }
    }
    void MoveToTargetPosition()
    {
        agent.SetDestination(currentTargetPosition);
    }

    IEnumerator TimerBeforeDestroy(int duration)
    {
        yield return new WaitForSeconds(duration); // Wait for the specified duration
        Destroy(gameObject); // Destroy the pickpocket after the timer expires
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Traffic Light"))
        {
            print("Traffic Light Triggered");
            agent.isStopped = true; // Stop the NPC when it enters the traffic light collider
        }
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Traffic Light"))
        {
            agent.isStopped = false; // Resume the NPC's movement when it exits the traffic light collider
        }
    }
}