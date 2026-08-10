/*
* Author: Zhi Hng
* Date: 10 August 2026
* Description: Calls the GameManager to change controller between player and car.
*/

using Ezereal;
using UnityEngine;
using UnityEngine.AI;

public class PlayerCarScript : MonoBehaviour
{
    [SerializeField] EzerealCarController carLogic;
    [HideInInspector] public Transform carBodyTransform;
    NavMeshObstacle carBodyNavObstacle;
    int walkableIndex;
    int lastArea = -1;
    void Start()
    {
        walkableIndex = NavMesh.GetAreaFromName("Walkable");
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
        NavMeshHit hit;
        if (NavMesh.SamplePosition(carBodyTransform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            int currentMask = hit.mask; // area mask of the surface

            if (currentMask != lastArea)
            {
                // Do whatever you need here
                if ((currentMask & (1 << walkableIndex)) == 0)
                {
                    print("Driving off road -5 points");
                    GameManager.Instance.AddScore(-5);
                }

                lastArea = currentMask; // update cache
            }
        }
    }
    
    void OnEsc()
    {
        GameManager.Instance.EscPressed();
    }
}
