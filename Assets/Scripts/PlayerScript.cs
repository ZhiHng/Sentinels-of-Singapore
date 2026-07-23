/*
* Author: Zhi Hng
* Date: 23 July 2026
* Description: Handles interactions between the player and interactable objects.
*/

using System; // Import standard .NET system types (not strictly needed here but common in C# files)
using UnityEngine; // Import Unity-specific classes like MonoBehaviour, GameObject, Collider, and print
using TMPro;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using System.Collections;

public class PlayerScript : MonoBehaviour
{
    GameObject currentHighlighted; // Stores the gameobject of the hovered object
    Material originalMaterial; // Stores the original material of the hovered object
    /// <summary>
    /// Reference the material to be used for highlighting of interactable objects
    /// </summary>
    [SerializeField]
    Material highlightMaterial;
    /// <summary>
    /// Objects in the highlightable layer will be highlighted when raycast hits it
    /// </summary>
    [SerializeField]
    LayerMask highlightable;

    void Start()
    {
        
    }
    void Update()
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 1.5f, highlightable)) { // Shoots a raycast out every frame to check if the object at the center of the camera is interactable
            GameObject target = hit.collider.gameObject;

            if (currentHighlighted != target) { // Changed interactable object
                
                // Remove highlight from old object
                if (currentHighlighted != null) {
                    Renderer[] childrenRenderer = currentHighlighted.GetComponentsInChildren<Renderer>();
                    foreach (Renderer child in childrenRenderer)
                    {
                        child.material = originalMaterial;
                    }
                }

                // Apply highlight to new object
                currentHighlighted = target;
                originalMaterial = currentHighlighted.GetComponentInChildren<Renderer>().material;
                Renderer[] childrenRenderers = currentHighlighted.GetComponentsInChildren<Renderer>();
                foreach (Renderer child in childrenRenderers)
                    {
                        child.material = highlightMaterial;
                    }
            }
        } else {
            // No hit, remove highlight from old object
            if (currentHighlighted != null) {
                Renderer[] childrenRenderer = currentHighlighted.GetComponentsInChildren<Renderer>();
                foreach (Renderer child in childrenRenderer)
                    {
                        child.material = originalMaterial;
                    }
                currentHighlighted = null;
            }
        }
    }
    void OnInteract() // Custom interaction method called when the player performs an interact action by clicking the key 'E"
    {
        if (Physics.Raycast(Camera.main.transform.position, Camera.main.transform.forward, out RaycastHit hit, 1.5f, highlightable)) { // Cast a ray to detect interactable objects only
            if(hit.collider.gameObject.CompareTag("DoorButton")) // Check if the object hit by the ray is tagged as a door
            {
                
            }
        }
    }
}

