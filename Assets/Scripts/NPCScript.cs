/*
* Author: Zhi Hng
* Date: 28 July 2026
* Description: Handles the AI for all the NPCs.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCScript : MonoBehaviour
{
    GameObject player;
    [HideInInspector] public bool hasCommitedCrime = false;
    [SerializeField] Material crimeMat;
    Material originalMat;
    Renderer npcRenderer;
    NavMeshAgent agent;
    [SerializeField] string npcType;
    public int scoreValue;
    Vector3 currentTargetPosition;
    Coroutine timerBeforeDestroyCoroutine;
    Coroutine walkingVariationCoroutine;
    Coroutine runFromPlayerCoroutine;
    int pathwayMask;
    int walkableMask;
    bool hasSeenPlayer = false;
    bool isRunning;
    bool isTired = false;
    float distanceBeforeStopRunning = 15;
    //PickPocket Variables
    GameObject targetCivilian;

    //Smoker Variables


    //Fighter Variables
    [HideInInspector] public Vector3 targetFightPosition;

    void Start()
    {
        player = GameObject.FindGameObjectWithTag("Player");
        npcRenderer = GetComponent<Renderer>();
        originalMat = npcRenderer.material;
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
            currentTargetPosition = targetFightPosition;
            MoveToTargetPosition();
        }

        // All npcs have variations in their movement. Can stop coroutine if AI is running.
        walkingVariationCoroutine = StartCoroutine(AddWalkingVariation());
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
                // remove delete 
                if (GetDistanceFromObject(player) < distanceBeforeStopRunning && timerBeforeDestroyCoroutine != null)
                {
                    StopCoroutine(timerBeforeDestroyCoroutine);
                    timerBeforeDestroyCoroutine = null;
                }
                else if (isTired)
                {
                    if (timerBeforeDestroyCoroutine == null) // If chased before and timer before destroy coroutine is stopped, will start it again.
                    {
                        timerBeforeDestroyCoroutine = StartCoroutine(TimerBeforeDestroy(10));
                    }
                }
                if (hasSeenPlayer)
                {
                    if (runFromPlayerCoroutine == null)
                    {
                        runFromPlayerCoroutine = StartCoroutine(RunFromPlayer());
                    }
                }
                else
                {
                    // Randomise running away chance
                    CastVisionCone(5, 60, 10);
                }
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
    /// Cast rays in a 2D cone shape
    /// </summary>
    /// <param name="rayCount">More rays means more accurate</param>
    /// <param name="coneAngle">How wide the cone is in degrees</param>
    /// <param name="visionRange">How far the cone reaches</param>
    void CastVisionCone(int rayCount, float coneAngle, float visionRange)
    {
        for (int i = 0; i < rayCount; i++) // Spread rays across the cone
        {
            float angle = Mathf.Lerp(-coneAngle / 2, coneAngle / 2, i / (float)(rayCount - 1)); // Changes angle every loop
            Vector3 dir = Quaternion.Euler(0, angle, 0) * transform.forward;

            if (Physics.Raycast(transform.position, dir, out RaycastHit hit, visionRange))
            {
                if (hit.collider.CompareTag("Player Collider"))
                {
                    hasSeenPlayer = true;
                    return;
                }
            }

            // Visualize rays in Scene view
            Debug.DrawRay(transform.position, dir * visionRange, Color.red);
        }
    }
    float GetDistanceFromObject(GameObject targetObject)
    {
        // Get positions
        Vector3 myPosition = transform.position;
        Vector3 targetPosition = targetObject.transform.position;

        // Calculate distance
        float distance = Vector3.Distance(myPosition, targetPosition);

        return distance;
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
            agent.isStopped = false;
            agent.speed = Random.Range(3f, 4f);
            yield return new WaitForSeconds(Random.Range(10, 15));
            switch (Random.Range(1, 7))
            {
                case 1 or 2:
                    agent.isStopped = true;
                    break;
                case 3 or 4 or 5:
                    agent.speed = Random.Range(1f, 4.5f);
                    break;
                case 6:
                    if (npcType == "Civilian") // Small chance to change destination
                    {
                        currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
                    }
                    break;
            }
            yield return new WaitForSeconds(Random.Range(5, 15));
            
        }
    }
    /// <summary>
    /// Runs away from the player. Gets tired and slow down the longer the run. Stops running when player is far enough. Deletes itself if far enough for some time. Returns to target destination once stopped running.
    /// </summary>
    /// <returns></returns>
    IEnumerator RunFromPlayer()
    {
        StopCoroutine(walkingVariationCoroutine); // Stop walking like a normal civilian
        agent.areaMask = walkableMask | pathwayMask;
        agent.isStopped = false;
        if (!isTired)
        {
            isTired = true;
            agent.speed = 6.5f;
        }
        float runDistance = 10f;
        isRunning = true;
        StartCoroutine(DecreaseSpeedOverTime(0.25f, 0.025f, 4f)); // Gets Tired after running for a while
        while (isRunning)
        {
            if (GetDistanceFromObject(player) > distanceBeforeStopRunning)
            {
                isRunning = false;
            }
            // Direction away from player
            Vector3 awayDir = (transform.position - player.transform.position).normalized;

            // Pick a point further away
            Vector3 runTo = transform.position + awayDir * runDistance;

            // Tell agent to go there
            agent.SetDestination(runTo);
            yield return new WaitForSeconds(0.25f); // Changes pathway every 0.25 sec
        }
        if (timerBeforeDestroyCoroutine == null)
        {
            timerBeforeDestroyCoroutine = StartCoroutine(TimerBeforeDestroy(10));
        }
        hasSeenPlayer = false;
        currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
        MoveToTargetPosition();
        runFromPlayerCoroutine = null;
    }
    /// <summary>
    /// Decreases agent speed over time
    /// </summary>
    /// <param name="interval">interval in seconds before each decrease</param>
    /// <param name="step">how much speed to decrease each time</param>
    /// <param name="minSpeed">Minimum speed to decrease until</param>
    /// <returns></returns>
    IEnumerator DecreaseSpeedOverTime(float interval, float step, float minSpeed)
    {
        while (agent.speed > minSpeed)
        {
            agent.speed = Mathf.Max(agent.speed - step, minSpeed);
            yield return new WaitForSeconds(interval);
        }
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Traffic Light") && !isTired)
        {
            agent.isStopped = true; // Stop the NPC when it enters the traffic light collider
        }
        if (other.name.Contains("Civilian") && gameObject.name.Contains("Pickpocket") && !hasCommitedCrime)
        {
            npcRenderer.material = crimeMat;
            hasCommitedCrime = true;
            currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
            MoveToTargetPosition();
        }
        if (walkingVariationCoroutine != null) StopCoroutine(walkingVariationCoroutine);
        walkingVariationCoroutine = null;
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Traffic Light") && !isTired)
        {
            agent.isStopped = false; // Resume the NPC's movement when it exits the traffic light collider
        }
        walkingVariationCoroutine = StartCoroutine(AddWalkingVariation());
    }
}