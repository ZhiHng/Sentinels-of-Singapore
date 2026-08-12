/*
* Author: Zhi Hng
* Date: 12 August 2026
* Description: Detects if the car crashes into infrastructure.
*/

using System;
using UnityEngine;

public class PlayerCarCrashingDetect : MonoBehaviour
{
    [SerializeField] int crashCount = 20;
    [SerializeField] PlayerCarScript playerCarScript;
    void OnCollisionEnter(Collision collision)
    {
        if (collision.gameObject.CompareTag("Infrastructure"))
        {
            GameManager.Instance.AddScore(-10);
            GameManager.Instance.BroadcastMessage("",3);
            crashCount--;
            if (crashCount == 0)
            {
                playerCarScript.DestroyCar();
            }
        }
    }
}
