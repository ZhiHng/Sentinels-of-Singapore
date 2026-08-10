/*
* Author: Zhi Hng
* Date: 10 August 2026
* Description: Handles interactions between the player and interactable objects.
*/

using UnityEngine;
using UnityEngine.AI; // Import Unity-specific classes like MonoBehaviour, GameObject, Collider, and print

public class PlayerScript : MonoBehaviour
{
    [SerializeField] AudioClip[] audioClips = new AudioClip[2];
    [SerializeField] LayerMask interactable;
    [SerializeField] NPCManager npcManager;
    [HideInInspector] public bool isResume = true;
    int pathwayIndex;
    int lastArea = -1;
    public bool isChasing = false;
    bool lastIsChasing = false;
    AudioSource audioSource;
    void Start()
    {
        GameManager.Instance.ResetToGameState();
        pathwayIndex = NavMesh.GetAreaFromName("Pathway");
        audioSource = GetComponent<AudioSource>();
        audioSource.volume = 0.1f;
    }
    void Update()
    {
        if (isChasing != lastIsChasing)
        {
            if (isChasing)
            {
                audioSource.clip = audioClips[1];
                audioSource.volume = 1;
            }
            else
            {
                audioSource.clip = audioClips[0];
                audioSource.volume = 0.05f;
            }
            lastIsChasing = isChasing;
        }
        NavMeshHit hit;
        if (NavMesh.SamplePosition(transform.position, out hit, 2.0f, NavMesh.AllAreas))
        {
            int currentMask = hit.mask; // area mask of the surface

            if (currentMask != lastArea)
            {
                // Do whatever you need here
                if ((currentMask & (1 << pathwayIndex)) == 0 && !isChasing)
                {
                    print("Jaywalking -5 points");
                    GameManager.Instance.AddScore(-5);
                }

                lastArea = currentMask; // update cache
            }
        }
    }
    void OnInteract() // Custom interaction method called when the player performs an interact action by clicking the key 'E"
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 1.5f, interactable)) { // Cast a ray to detect interactable objects only
            if (hit.collider.gameObject.CompareTag("NPC"))
            {
                NPCScript script = hit.collider.gameObject.GetComponent<NPCScript>();
                if (script.hasCommitedCrime)
                {
                    GameManager.Instance.AddScore(script.scoreValue);
                    Destroy(hit.collider.gameObject);
                }
                else
                {
                    GameManager.Instance.AddScore(-10); // Lose score if NPC did not commit a crime.
                }
            }
            if (hit.collider.gameObject.CompareTag("Player Car"))
            {
                GameManager.Instance.HidePlayer();
            }
            if (hit.collider.gameObject.CompareTag("Fight Cloud"))
            {
                hit.collider.gameObject.GetComponent<FightCloud>().Interacted();
            }
        }
    }
    void OnEnter()
    {
        if (GameManager.Instance.isEndScreen)
        {
            GameManager.Instance.isEndScreen = false;
            GameManager.Instance.ResetToMainMenu(npcManager.level);
        }
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
    void OnWASD()
    {
        if (GameManager.Instance.isPauseMenu)
        {
            isResume = !isResume;
            GameManager.Instance.ChangePauseMenuScroll(isResume);
        }
    }
    void OnEsc()
    {
        GameManager.Instance.EscPressed();
    }
}

