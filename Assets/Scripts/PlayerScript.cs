/*
* Author: Zhi Hng
* Date: 31 July 2026
* Description: Handles interactions between the player and interactable objects.
*/

using UnityEditor.SearchService;
using UnityEngine; // Import Unity-specific classes like MonoBehaviour, GameObject, Collider, and print

public class PlayerScript : MonoBehaviour
{
    [SerializeField] LayerMask interactable;
    [SerializeField] GameManager gameManager;
    void OnInteract() // Custom interaction method called when the player performs an interact action by clicking the key 'E"
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 1.5f, interactable)) { // Cast a ray to detect interactable objects only
            if (hit.collider.gameObject.CompareTag("NPC"))
            {
                NPCScript script = hit.collider.gameObject.GetComponent<NPCScript>();
                if (script.hasCommitedCrime)
                {
                    gameManager.AddScore(script.scoreValue);
                    Destroy(hit.collider.gameObject);
                }
                else
                {
                    gameManager.AddScore(-10); // Lose score if NPC did not commit a crime.
                }
            }
            if (hit.collider.gameObject.CompareTag("Player Car"))
            {
                gameManager.HidePlayer();
            }
        }
    }
}

