/*
* Author: Zhi Hng
* Date: 8 August 2026
* Description: Handles interactions between the player and interactable objects.
*/

using UnityEngine; // Import Unity-specific classes like MonoBehaviour, GameObject, Collider, and print

public class PlayerScript : MonoBehaviour
{
    [SerializeField] LayerMask interactable;
    [SerializeField] NPCManager npcManager;
    [HideInInspector] public bool isResume = true;
    void Start()
    {
        GameManager.Instance.ResetToGameState();
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

