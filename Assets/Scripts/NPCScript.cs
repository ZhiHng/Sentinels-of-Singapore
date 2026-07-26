/*
* Author: Zhi Hng
* Date: 26 July 2026
* Description: Handles the AI for all the NPCs.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCScript : MonoBehaviour
{
    [HideInInspector] public bool hasCommitedCrime = false;
    [SerializeField] Material crimeMat;
    Material originalMat;
    Renderer renderer;
    NavMeshAgent agent;
    [SerializeField] string npcType;
    public int scoreValue;
    Vector3 currentTargetPosition;
    Coroutine timerBeforeDestroyCoroutine;
    Coroutine walkingVariation;
    int pathwayMask;
    int walkableMask;
    //PickPocket Variables
    GameObject targetCivilian;

    //Smoker Variables


    //Fighter Variables


    void Start()
    {
        renderer = GetComponent<Renderer>();
        originalMat = renderer.material;
        pathwayMask = 1 << NavMesh.GetAreaFromName("Pathway");
        walkableMask = 1 << NavMesh.GetAreaFromName("Walkable");
        agent = GetComponent<NavMeshAgent>();
        agent.areaMask = pathwayMask; // Use to switch which nav mesh surface to use

        // Get a random target destination depending on npc type
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

        // All npcs have variations in their movement. Can stop coroutine if AI is running.
        walkingVariation = StartCoroutine(AddWalkingVariation());
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
            if (!hasCommitedCrime)
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
            else
            {
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    Destroy(gameObject);
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
    /// <summary>
    /// Moves to the Vector3 stored in currentTargetPosition
    /// </summary>
    void MoveToTargetPosition()
    {
        agent.SetDestination(currentTargetPosition);
    }
    /// <summary>
    /// Deletes gameObject after some seconds. Can start coroutine with timerBeforeDestroyCoroutine and stopping if an event occured.
    /// </summary>
    /// <param name="duration">Time in seconds before delete</param>
    /// <returns></returns>
    IEnumerator TimerBeforeDestroy(int duration)
    {
        yield return new WaitForSeconds(duration); // Wait for the specified duration
        Destroy(gameObject); // Destroy the pickpocket after the timer expires
    }
    /// <summary>
    /// Adds variation in movement, including, speed, stopping and changing target position.
    /// </summary>
    /// <returns></returns>
    IEnumerator AddWalkingVariation()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(10, 15));
            switch (Random.Range(1, 10))
            {
                case 1 or 2 or 3 or 4:
                    agent.isStopped = true;
                    break;
                case 5 or 6 or 7 or 8 or 9:
                    agent.speed = Random.Range(1f, 4.5f);
                    break;
                case 10:
                    if (npcType == "Civilian") // Small chance to change destination
                    {
                        currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
                    }
                    break;
            }
            yield return new WaitForSeconds(Random.Range(5, 15));
            agent.isStopped = false;
            agent.speed = 3.5f;
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Traffic Light"))
        {
            agent.isStopped = true; // Stop the NPC when it enters the traffic light collider
        }
        if (other.name.Contains("Civilian") && gameObject.name.Contains("Pickpocket") && !hasCommitedCrime)
        {
            renderer.material = crimeMat;
            hasCommitedCrime = true;
            currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
            MoveToTargetPosition();
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