/*
* Author: Zhi Hng
* Date: 11 August 2026
* Description: Handles the AI for all the NPCs.
*/

using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class NPCScript : MonoBehaviour
{
    ParticleSystem particleSystem;
    [SerializeField] AudioClip[] shoutAudio = new AudioClip[2];
    AudioSource audioSource;
    Animator animator;
    GameObject player;
    Rigidbody npcRigidbody;
    public bool hasCommitedCrime = false;
    // [SerializeField] Material crimeMat;
    // Material originalMat;
    // Renderer npcRenderer;
    NavMeshAgent agent;
    public string npcType;
    public int scoreValue;
    Vector3 currentTargetPosition;
    Coroutine timerBeforeDestroyCoroutine;
    [HideInInspector] public Coroutine walkingVariationCoroutine;
    Coroutine runFromPlayerCoroutine;
    int pathwayMask;
    int walkableMask;
    bool hasSeenPlayer = false;
    bool isAttemptingToRun = false;
    bool isRunning;
    bool isTired = false;
    float distanceBeforeStopRunning = 15;
    [HideInInspector] public BeaconScript isTrackedScript;
    //PickPocket Variables
    GameObject targetCivilian;

    //Smoker Variables
    bool hasStartedSmoking = false;
    bool isSmoking = false;
    bool isLookingAround = false;
    bool hasFinishedSmoking = false;
    float patience = 10f; // Seconds willing to wait before smoking
    float playerScareDistance = 30f;
    Coroutine smokingCoroutine;
    Coroutine lookAroundCoroutine;
    Coroutine patienceCoroutine;
    [SerializeField] GameObject vape;

    // Fighter Variables
    [HideInInspector] public Vector3 targetFightPosition;

    [SerializeField] GameObject fightCloudPrefab;
    [SerializeField] float waitForPartnerTime = 120f;
    [SerializeField] float fightDuration = 60f;
    [SerializeField] int minimumFightScore = 0;
    [SerializeField] int scoreLostPerSeverity = 1;

    NPCScript partner;
    bool hasWaited = false;
    bool waiting;
    bool fighting;
    bool leader;
    bool fightResolved = false;

    GameObject fightCloud;

    int severity;

    Coroutine waitCoroutine;
    Coroutine severityCoroutine;
    Coroutine fightCoroutine;

    void Start()
    {
        particleSystem = GetComponentInChildren<ParticleSystem>();
        animator = GetComponent<Animator>();
        npcRigidbody = GetComponent<Rigidbody>();
        player = GameObject.FindGameObjectWithTag("Player");
        // npcRenderer = GetComponent<Renderer>();
        // originalMat = npcRenderer.material;
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
        else if (npcType == "Pickpocket")
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
            audioSource = GetComponent<AudioSource>();
            currentTargetPosition = targetFightPosition;
            MoveToTargetPosition();
        }

        // All npcs have variations in their movement. Can stop coroutine if AI is running.
        walkingVariationCoroutine = StartCoroutine(AddWalkingVariation());
    }
    void Update()
    {
        if (!agent.isStopped != animator.GetBool("isWalking"))
        {
            if (agent.isStopped) particleSystem.Stop();
            animator.SetBool("isWalking", !agent.isStopped);
        }
        if (animator.GetBool("isRunning") != agent.speed >= 5) // Only changes when speed goes over or below threshold.
        {
            if (agent.speed >= 5) 
            {
                particleSystem.Play();
            }
            else
            {
                particleSystem.Stop();
            }
            animator.SetBool("isRunning", agent.speed >= 5);
        }
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
                if (hasSeenPlayer)
                {
                    if (runFromPlayerCoroutine == null)
                    {
                        runFromPlayerCoroutine = StartCoroutine(RunFromPlayer());
                    }
                }
                else
                {
                    CastVisionCone(5, 60, 10);
                }
                if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance)
                {
                    if (hasCommitedCrime) NPCManager.offenders.Remove(gameObject);
                    if (isTrackedScript != null) isTrackedScript.Despawn();
                    Destroy(gameObject);
                }
            }
        }
        else if (npcType == "Smoker")
        {
            if (hasFinishedSmoking &&
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance)
            {
                if (hasCommitedCrime) NPCManager.offenders.Remove(gameObject);
                if (isTrackedScript != null) isTrackedScript.Despawn();
                Destroy(gameObject);
            }
            // Reached smoking location
            if (!hasStartedSmoking &&
                agent.hasPath &&
                agent.remainingDistance <= agent.stoppingDistance &&
                !agent.pathPending)
            {
                print("pls smoke");
                hasStartedSmoking = true;
                agent.isStopped = true;

                float distance = GetDistanceFromObjectVector(player.transform.position);

                // Player is far enough away
                if (distance > playerScareDistance)
                {
                    StartSmoking();
                }
                else
                {
                    // Player is nearby
                    if (patienceCoroutine == null)
                        patienceCoroutine = StartCoroutine(WaitForPlayerToLeave());
                }
            }

            // While smoking, keep checking vision
            if (hasCommitedCrime)
            {
                CastVisionCone(5, 80, 12);
            }

            // Player spotted
            if (hasSeenPlayer)
            {
                if (runFromPlayerCoroutine == null)
                {
                    StopSmoking();
                    runFromPlayerCoroutine = StartCoroutine(RunFromPlayer());
                }
            }
        }
        else if (npcType == "Fighter")
        {
            // Check if the fighter has reached the fight position
            if (!waiting &&
                !fighting &&
                !agent.pathPending &&
                agent.remainingDistance <= agent.stoppingDistance && !hasWaited)
            {
                ReachFightPosition();
            }
            if (!agent.pathPending && agent.remainingDistance <= agent.stoppingDistance && !waiting && !fighting && hasWaited)
            {
                Destroy(gameObject);
            }
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

            if (Physics.Raycast(transform.position + Vector3.up, dir, out RaycastHit hit, visionRange))
            {
                if (hit.collider.CompareTag("Player Collider"))
                {
                    hasSeenPlayer = true;
                    return;
                }
            }

            // Visualize rays in Scene view
            Debug.DrawRay(transform.position + Vector3.up, dir * visionRange, Color.red);
        }
    }
    float GetDistanceFromObjectVector(Vector3 targetPosition)
    {
        // Get positions
        Vector3 myPosition = transform.position;

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
        if (npcType == "Civilian") NPCManager.spawnedCivilians.Remove(gameObject); // Remove the civilian from the list of spawned civilians
        if (npcType == "Pickpocket" || npcType == "Smoker" || npcType == "Fighter" && hasCommitedCrime) NPCManager.offenders.Remove(gameObject);
        Destroy(gameObject);
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
        int run = Random.Range(0,3); // Randomise chance to run when seeing player.
        switch (run)
        {
            case 0 or 1:
                isAttemptingToRun = true;
                break;
            case 2:
                isAttemptingToRun = false;
                break;
        }
        if (isAttemptingToRun)
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
            player.GetComponent<PlayerScript>().isChasing = true;
            while (isRunning)
            {
                if (GetDistanceFromObjectVector(player.transform.position) > distanceBeforeStopRunning)
                {
                    player.GetComponent<PlayerScript>().isChasing = false;
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
            hasSeenPlayer = false;
            currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
            MoveToTargetPosition();
            runFromPlayerCoroutine = null;
        }
        else
        {
            while (GetDistanceFromObjectVector(player.transform.position) < distanceBeforeStopRunning)
            {
                yield return null;
            }
            hasSeenPlayer = false;
            runFromPlayerCoroutine = null;
        }
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
        if (other.CompareTag("Traffic Light") && !isTired) // !isTired means the NPC is an offender exposed and trying to escape
        {
            if (GetDistanceFromObjectVector(other.bounds.center) < 9f) // Runs if is changing traffic light and in the middle of the road.
            {
                agent.speed = 5.5f;
            }
            else
            {
                agent.isStopped = true; // Stop the NPC when it enters the traffic light collider
            }
        }
        if (other.CompareTag("NPC") && other.gameObject.GetComponent<NPCScript>().npcType == "Civilian" && npcType == "Pickpocket" && !hasCommitedCrime)
        {
            int ran = Random.Range(0,3);
            switch (ran)
            {
                case 0 or 1:
                    if (other.gameObject.name.Contains("Civilian1"))
                    {
                        AudioSource.PlayClipAtPoint(shoutAudio[0], transform.position);
                    }
                    else
                    {
                        AudioSource.PlayClipAtPoint(shoutAudio[1], transform.position);
                    }
                    break;
            }
            // npcRenderer.material = crimeMat;
            NPCManager.offenders.Add(gameObject);
            hasCommitedCrime = true;
            currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
            MoveToTargetPosition();
        }
        if (walkingVariationCoroutine != null) StopCoroutine(walkingVariationCoroutine);
        walkingVariationCoroutine = null;

        if (other.gameObject.CompareTag("Player Car"))
        {
            if (walkingVariationCoroutine != null) StopCoroutine(walkingVariationCoroutine);
            walkingVariationCoroutine = null;

            agent.isStopped = true;
            agent.enabled = false;
            animator.SetBool("isWalking", false);
            animator.enabled = false;
            npcRigidbody.isKinematic = false;
            npcRigidbody.useGravity = true;
            Vector3 relativeDirection = (other.transform.position - transform.position).normalized;
            npcRigidbody.AddForce(relativeDirection * 150f, ForceMode.Impulse);
            if (timerBeforeDestroyCoroutine == null) 
            {
                GameManager.Instance.AddScore(-20);
                timerBeforeDestroyCoroutine = StartCoroutine(TimerBeforeDestroy(5));
            }
        }
        if (other.gameObject.CompareTag("NPC") && other.GetComponent<NPCScript>().npcType == "Fighter" && npcType == "Fighter")
        {
            if (fighting)
            return;

            NPCScript otherNpc = other.GetComponent<NPCScript>();
            if (otherNpc == null) return;
            if (otherNpc.npcType != "Fighter") return;

            // Prevent duplicate fights
            if (fighting || otherNpc.fighting) return;

            // Decide who should start the fight: always the fighter with the lower ID
            if (GetInstanceID() < otherNpc.GetInstanceID())
            {
                StartFight(otherNpc); // only one fighter runs this
            }
        }
        
    }
    void OnTriggerExit(Collider other)
    {
        if (other.CompareTag("Traffic Light") && !isTired && !fighting)
        {
            agent.isStopped = false;
        }

        if (!fighting && walkingVariationCoroutine == null)
        {
            walkingVariationCoroutine = StartCoroutine(AddWalkingVariation());
        }
    }
    IEnumerator WaitForPlayerToLeave()
    {
        while (!isSmoking)
        {
            float distance = GetDistanceFromObjectVector(player.transform.position);

            if (distance > playerScareDistance)
            {
                StartSmoking();
                break;
            }

            patience -= Time.deltaTime;

            if (patience <= 0)
            {
                StartSmoking();
                break;
            }

            yield return null;
        }

        patienceCoroutine = null;
    }

    IEnumerator LookAround()
    {
        while (true)
        {
            yield return new WaitForSeconds(Random.Range(3f, 5f));

            float angle = Random.Range(-90f, 90f);
            Quaternion startRotation = transform.rotation;
            Quaternion targetRotation = Quaternion.Euler(0, angle, 0) * startRotation;

            float duration = 1f; // how long the turn should take
            float elapsed = 0f;

            while (elapsed < duration)
            {
                transform.rotation = Quaternion.Lerp(startRotation, targetRotation, elapsed / duration);
                elapsed += Time.deltaTime;
                yield return null;
            }

            transform.rotation = targetRotation; // snap to final rotation
            yield return new WaitForSeconds(1f);
        }
    }

    IEnumerator Smoke()
    {
        agent.isStopped = true;
        yield return new WaitForSeconds(30f);

        StopSmoking();

        hasFinishedSmoking = true;

        currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
        agent.isStopped = false;
        MoveToTargetPosition();
    }

    void StartSmoking()
    {
        if (isSmoking) return;

        isSmoking = true;
        vape.SetActive(true);

        // npcRenderer.material = crimeMat;
        NPCManager.offenders.Add(gameObject);
        hasCommitedCrime = true;

        smokingCoroutine = StartCoroutine(Smoke());
        lookAroundCoroutine = StartCoroutine(LookAround());
    }

    void StopSmoking()
    {
        isSmoking = false;
        vape.SetActive(false);

        if (lookAroundCoroutine != null)
            StopCoroutine(lookAroundCoroutine);

        smokingCoroutine = null;
    }

    /// <summary>
    /// Called when a fighter reaches the fight location.
    /// </summary>
    void ReachFightPosition()
    {
        waiting = true;
        hasWaited = true;

        agent.isStopped = true;

        StopCoroutine(walkingVariationCoroutine);
        walkingVariationCoroutine = null;
        
        waitCoroutine = StartCoroutine(WaitForPartner());
    }

    /// <summary>
    /// Starts the fight between the two fighters.
    /// </summary>
    public void StartFight(NPCScript other)
    {
        fighting = true;
        waiting = false;

        partner = other;

        other.partner = this;
        other.fighting = true;
        other.waiting = false;

        agent.isStopped = true;
        other.agent.isStopped = true;

        // npcRenderer.material = crimeMat;
        // other.npcRenderer.material = crimeMat;

        DisableChildren(gameObject);
        DisableChildren(other.gameObject);

        if (waitCoroutine != null)
            StopCoroutine(waitCoroutine);

        if (other.waitCoroutine != null)
            StopCoroutine(other.waitCoroutine);

        if (walkingVariationCoroutine != null)
        {
            StopCoroutine(walkingVariationCoroutine);
            walkingVariationCoroutine = null;
        }

        if (other.walkingVariationCoroutine != null)
        {
            StopCoroutine(other.walkingVariationCoroutine);
            other.walkingVariationCoroutine = null;
        }

        leader = GetInstanceID() < other.GetInstanceID();

        other.leader = !leader;

        agent.isStopped = true;
        other.agent.isStopped = true;

        if (leader)
        {
            NPCManager.offenders.Add(gameObject);
            SpawnFightCloud();

            severityCoroutine = StartCoroutine(IncreaseSeverity());

            fightCoroutine = StartCoroutine(FightTimeout());
        }
    }

    void DisableChildren(GameObject parent)
    {
        foreach (Transform child in parent.transform)
        {
            child.gameObject.SetActive(false);
        }
    }

    /// <summary>
    /// Creates the cloud VFX between both fighters.
    /// </summary>
    void SpawnFightCloud()
    {
        Vector3 pos = (transform.position + partner.transform.position) * 0.5f;

        fightCloud = Instantiate(fightCloudPrefab, pos, Quaternion.identity);
        audioSource.Play();
        fightCloud.GetComponent<FightCloud>()
                .Initialise(this);
    }

    /// <summary>
    /// Ends the fight and destroys both fighters.
    /// </summary>
    public void ResolveFight(bool playerStoppedFight)
    {
        if (fightResolved)
            return;

        fightResolved = true;
        if (playerStoppedFight)
        {
            int score =
                Mathf.Max(
                    minimumFightScore,
                    scoreValue - severity * scoreLostPerSeverity);

            GameManager.Instance.AddScore(score);
        }
        if (severityCoroutine != null)
        StopCoroutine(severityCoroutine);

        if (fightCoroutine != null)
            StopCoroutine(fightCoroutine);

        if (fightCloud != null)
            Destroy(fightCloud);

        if (partner != null)
            Destroy(partner.gameObject);

        NPCManager.offenders.Remove(gameObject);
        Destroy(gameObject);
    }

    IEnumerator WaitForPartner()
    {
        yield return new WaitForSeconds(waitForPartnerTime);

        if (fighting)
            yield break;

        waiting = false;

        agent.isStopped = false;
        currentTargetPosition = NPCManager.targetPoints[Random.Range(0, NPCManager.targetPoints.Length)].position;
        if (walkingVariationCoroutine == null)
            walkingVariationCoroutine = StartCoroutine(AddWalkingVariation());
        MoveToTargetPosition();
    }
    IEnumerator IncreaseSeverity()
    {
        severity = 0;

        while (true)
        {
            yield return new WaitForSeconds(1);

            severity++;
        }
    }
    IEnumerator FightTimeout()
    {
        yield return new WaitForSeconds(fightDuration);
        ResolveFight(false);
    }
}