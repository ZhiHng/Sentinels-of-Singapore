/*
* Author: Zhi Hng
* Date: 12 August 2026
* Description: Detect if the Player Car has run a red traffic light.
*/

using UnityEngine;

public class PlayerCarTrafficLightDetect : MonoBehaviour
{
    void OnTriggerEnter(Collider other)
    {
        if (other.CompareTag("Traffic Light"))
        {
            GameManager.Instance.AddScore(-5);
            print("Player Car hit a traffic light! Score decreased by 5.");
            GameManager.Instance.BroadcastMessage("",7);
        }
    }
}
