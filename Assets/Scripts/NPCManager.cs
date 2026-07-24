/*
* Author: Zhi Hng
* Date: 24 July 2026
* Description: Spawns NPCs.
*/

using UnityEngine;
using System.Collections.Generic;

public class NPCManager : MonoBehaviour
{
    [SerializeField] int numberOfEnemies;
    [SerializeField] int numberOfCivilians;
    [SerializeField] GameObject pickPocketPrefab;
    [SerializeField] GameObject smokerPrefab;
    [SerializeField] GameObject fighterPrefab;
    [SerializeField] GameObject civilianPrefab;
    [SerializeField] int playTime;
    public List<NPCScript> enemies = new List<NPCScript>();
    float timer = 0f;
    int enemiesToSpawnEachRound;
    int civiliansToSpawnEachRound;
    void Start()
    {
        enemiesToSpawnEachRound = numberOfEnemies / playTime;
        civiliansToSpawnEachRound = numberOfCivilians / playTime;
        int remainderEnemies = numberOfEnemies % playTime;
        int remainderCivilians = numberOfCivilians % playTime;
        SpawnEnemies(enemiesToSpawnEachRound + remainderEnemies); // Spawn the remainder of the enemies in the first round
        SpawnCivilians(civiliansToSpawnEachRound + remainderCivilians); // Spawn the remainder of the civilians in the first round
    }
    void Update()
    {
        timer += Time.deltaTime;

        // Convert timer to whole seconds
        int seconds = Mathf.FloorToInt(timer);

        // Check if it's a multiple of 60 (every minute)
        if (seconds % 60 == 0 && seconds != 0)
        {
            SpawnEnemies(enemiesToSpawnEachRound);
            SpawnCivilians(civiliansToSpawnEachRound);
        }
    }
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
                    GameObject newEnemy = Instantiate(enemyPrefab, new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f)), Quaternion.identity);
                }
            }
    }
    void SpawnCivilians(int numberToSpawn)
    {
        for (int i = 0; i < numberToSpawn; i++)
        {
            GameObject newCivilian = Instantiate(civilianPrefab, new Vector3(Random.Range(-10f, 10f), 0.5f, Random.Range(-10f, 10f)), Quaternion.identity);
        }
    }
}
