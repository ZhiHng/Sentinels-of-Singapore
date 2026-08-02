/*
* Author: Zhi Hng
* Date: 2 August 2026
* Description: Detect if the Player Car has run a red traffic light.
*/

using UnityEngine;

public class PlayerCarTrafficLightDetect : MonoBehaviour
{
    GameManager gameManager;
    void Start()
    {
        gameManager = Object.FindFirstObjectByType<GameManager>();
    }
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Traffic Light"))
        {
            gameManager.AddScore(-5);
            print("Player Car hit a traffic light! Score decreased by 5.");
        }
    }
}
