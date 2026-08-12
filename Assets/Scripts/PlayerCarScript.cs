/*
* Author: Zhi Hng
* Date: 12 August 2026
* Description: Calls the GameManager to change controller between player and car.
*/

using Ezereal;
using UnityEngine;
using UnityEngine.AI;

public class PlayerCarScript : MonoBehaviour
{
    [SerializeField] EzerealCarController carLogic;
    EzerealSoundController carSoundLogic;
    [SerializeField] GameObject burningVFX;
    public Transform carBodyTransform;
    NavMeshObstacle carBodyNavObstacle;
    int walkableIndex;
    int lastArea = -1;
    [HideInInspector] public bool isResume = true;
    public bool isCarDestroyed = false;
    void Start()
    {
        walkableIndex = NavMesh.GetAreaFromName("Walkable");
        carBodyNavObstacle = GameObject.FindGameObjectWithTag("Player Car").GetComponent<NavMeshObstacle>();
        carSoundLogic = gameObject.GetComponent<EzerealSoundController>();
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
                    GameManager.Instance.BroadcastMessage("",6);
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
    void On_1()
    {
        GameManager.Instance.Clicked1();
    }
    void On_2()
    {
        GameManager.Instance.Clicked2();
    }
    void OnWASD()
    {
        if (GameManager.Instance.isPauseMenu)
        {
            isResume = !isResume;
            GameManager.Instance.ChangePauseMenuScroll(isResume);
        }
    }
    void OnEnter()
    {
        if (GameManager.Instance.isPauseMenu)
        {
            if (isResume)
            {
                GameManager.Instance.EscPressed();
            }
            else
            {
                GameManager.Instance.ResetToMainMenu(-1);
            }
        }
    }
    public void DestroyCar()
    {
        isCarDestroyed = true;
        burningVFX.SetActive(true);
        GameManager.Instance.HidePlayerCar();
    }
}
