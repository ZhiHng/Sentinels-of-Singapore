/*
* Author: Zhi Hng
* Date: 6 August 2026
* Description: Spawns NPCs.
*/

using UnityEngine;
using System.Collections.Generic;
using System.Collections;

public class NPCManager : MonoBehaviour
{
    [SerializeField] GameObject directionalLight;
    float degreeToTurnLight;
    [HideInInspector] public static Transform[] targetPoints;
    [HideInInspector] public static Transform[] eventPoints;
    [SerializeField] int numberOfEnemies;
    [SerializeField] int civiliansSpawnInterval;
    [SerializeField] int maxCiviliansAtOneTime;
    int numberOfCivilians;
    [SerializeField] GameObject pickPocketPrefab;
    [SerializeField] GameObject smokerPrefab;
    [SerializeField] GameObject fighterPrefab;
    [SerializeField] GameObject civilianPrefab;
    [SerializeField] int playTime;
    public static List<GameObject> spawnedCivilians = new List<GameObject>();
    float timer = 0f;
    int enemiesToSpawnEachRound;
    Coroutine spawnEnemyCoroutine;
    int level = 1; // Current level of the game, starting from level 1
    int graphicQuality = 0;
    int difficulty = 0;
    void Start()
    {
        if (MainMenuManager.noOfEnemies != 0)
        {
            // Use the values from MainMenuManager
            numberOfEnemies = MainMenuManager.noOfEnemies;
            civiliansSpawnInterval = MainMenuManager.civilianSpawnInterval;
            maxCiviliansAtOneTime = MainMenuManager.maxCivilians;
            playTime = MainMenuManager.playTime;
            level = MainMenuManager.level;
            graphicQuality = MainMenuManager.graphicQuality;
            difficulty = MainMenuManager.difficulty;
            print("Level:" + level + " Graphic Quality:" + graphicQuality + " Difficulty:" + difficulty);
        }
        directionalLight.transform.rotation = Quaternion.Euler(50f, 0, 0);
        degreeToTurnLight = (180 - 50) / (8 * 60); // (start degree - end degree) / (minutes in seconds)
        // Gets all spawn points and event points placed in unity editor allowing for quick modification of points
        GameObject[] spawnPointObjects = GameObject.FindGameObjectsWithTag("Spawn Point");
        targetPoints = new Transform[spawnPointObjects.Length];
        for (int i = 0; i < spawnPointObjects.Length; i++)
        {
            targetPoints[i] = spawnPointObjects[i].transform;
            spawnPointObjects[i].SetActive(false); // Disable the spawn point objects after storing their transforms
        }
        GameObject[] eventPointObjects = GameObject.FindGameObjectsWithTag("Event Point");
        eventPoints = new Transform[eventPointObjects.Length];
        for (int i = 0; i < eventPointObjects.Length; i++)
        {
            eventPoints[i] = eventPointObjects[i].transform;
            eventPointObjects[i].SetActive(false); // Disable the event point objects after storing their transforms
        }

        enemiesToSpawnEachRound = numberOfEnemies / playTime;
        int remainderEnemies = numberOfEnemies % playTime;
        spawnEnemyCoroutine = StartCoroutine(SpawnEnemiesOverTime(enemiesToSpawnEachRound + remainderEnemies, 1f)); // Spawn the remainder of the enemies in the first round
        StartCoroutine(SpawnCiviliansOverTime(civiliansSpawnInterval)); // Spawn civilians over time
    }
    void Update()
    {
        directionalLight.transform.Rotate(degreeToTurnLight * Time.deltaTime, 0, 0);
        timer += Time.deltaTime;

        // Convert timer to whole seconds
        int seconds = Mathf.FloorToInt(timer);

        // Check if it's a multiple of 60 (every minute)
        if (seconds % 60 == 0 && seconds != 0 && seconds / 60 <= playTime)
        {
            if (spawnEnemyCoroutine == null)
            {
                spawnEnemyCoroutine = StartCoroutine(SpawnEnemiesOverTime(enemiesToSpawnEachRound, 1f)); // Spawn enemies over time
            }
        }
    }
    /// <summary>
    /// Spawns enemies randomly
    /// </summary>
    /// <param name="numberToSpawn">Number of enemies to spawn at one time</param>
    void SpawnEnemies(int numberToSpawn)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            int randomEnemyType = Random.Range(0, 3); // Randomly choose between 0, 1, or 2
            GameObject enemyPrefab = null;

            switch (randomEnemyType)
            {
                case 0:
                    enemyPrefab = pickPocketPrefab;
                    break;
                case 1:
                    enemyPrefab = smokerPrefab;
                    break;
                case 2:
                    enemyPrefab = fighterPrefab;
                    break;
            }

            if (enemyPrefab != null)
            {
                Vector3 fightPosition = eventPoints[Random.Range(0, NPCManager.eventPoints.Length)].position;
                GameObject newEnemy;
                if (enemyPrefab == fighterPrefab && i <= numberToSpawn - 1) // Check if it's a fighter and not the last enemy to spawn
                {
                    // Spawn the fighter at a random event point
                    newEnemy = Instantiate(enemyPrefab, targetPoints[Random.Range(0, targetPoints.Length)].position, Quaternion.identity);
                    newEnemy.GetComponent<NPCScript>().targetFightPosition = fightPosition; // Tells the extra fighter where to go
                    i++; // Increment i to account for the extra fighter spawned
                }
                else if (i == numberToSpawn)
                {
                    enemyPrefab = pickPocketPrefab; // Ensure the last enemy is a PickPocket
                }
                newEnemy = Instantiate(enemyPrefab, targetPoints[Random.Range(0, targetPoints.Length)].position, Quaternion.identity);
                if (enemyPrefab == fighterPrefab) newEnemy.GetComponent<NPCScript>().targetFightPosition = fightPosition; // Tells the fighter where the previous fighter went
            }
        }
    }
    /// <summary>
    /// Spawns civilians randomly
    /// </summary>
    /// <param name="numberToSpawn">Number of civilians to spawn at one time</param>
    void SpawnCivilians(int numberToSpawn)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            GameObject newCivilian = Instantiate(civilianPrefab, targetPoints[Random.Range(0, targetPoints.Length)].position, Quaternion.identity);
            spawnedCivilians.Add(newCivilian);
        }
    }
    /// <summary>
    /// Spawn civilians at a random rate around the interval set. Spawning 1 civilain each time.
    /// </summary>
    /// <param name="aroundInterval">Spawn rate will be kept close to the set interval</param>
    /// <returns></returns>
    IEnumerator SpawnCiviliansOverTime(float aroundInterval)
    {
        while (true)
        {
            if (spawnedCivilians.Count < maxCiviliansAtOneTime)
            {
                SpawnCivilians(1); // Spawn one civilian at a time
                numberOfCivilians++;
            }
            yield return new WaitForSeconds(Random.Range(Mathf.Max(aroundInterval - 1f, 0f), aroundInterval + 1f)); // Wait for a random interval around the specified time
        }
    }
    /// <summary>
    /// Spawn enemies at a random rate around the interval set. Spawning 1 enemy each time.
    /// </summary>
    /// <param name="numberToSpawn"><Number of enemies to spawn in total/param>
    /// <param name="aroundInterval">Spawn rate will be kept close to the set interval</param>
    /// <returns></returns>
    IEnumerator SpawnEnemiesOverTime(int numberToSpawn, float aroundInterval)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            SpawnEnemies(1); // Spawn one enemy at a time
            yield return new WaitForSeconds(Random.Range(aroundInterval - 1f, aroundInterval + 1f)); // Wait for a random interval around the specified time
        }
        spawnEnemyCoroutine = null; // Reset the coroutine reference after spawning all enemies
    }
}
