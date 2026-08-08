/*
* Author: Zhi Hng
* Date: 31 July 2026
* Description: Calls the GameManager to change controller between player and car.
*/

using Ezereal;
using UnityEngine;
using UnityEngine.AI;

public class PlayerCarScript : MonoBehaviour
{
    [SerializeField] EzerealCarController carLogic;
    NavMeshObstacle carBodyNavObstacle;
    void Start()
    {
        carBodyNavObstacle = GameObject.FindGameObjectWithTag("Player Car").GetComponent<NavMeshObstacle>();
    }
    void OnTab()
    {
        if (carLogic.currentSpeed < 0.1)
        {
            carLogic.currentSpeed = 0f;
            GameManager.Instance.HidePlayerCar();
        }
    }
    void Update()
    {
        if (carLogic.currentSpeed < 0.1)
        {
            carBodyNavObstacle.enabled = true;
        }
        else
        {
            carBodyNavObstacle.enabled = false;
        }
    }
}
