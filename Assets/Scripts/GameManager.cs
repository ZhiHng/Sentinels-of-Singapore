/*
* Author: Zhi Hng
* Date: 31 July 2026
* Description: Handles management between scenes and player score.
*/

using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    int currentScore;
    [SerializeField] GameObject playerParent;
    [SerializeField] TextMeshProUGUI scoreText; // Reference to the UI text element that displays the player's score
    [SerializeField] GameObject[] carObjectsToDisable = new GameObject[3];
    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            scoreText.text = "Score: " + 0;
        }
        else
        {
            Destroy(gameObject);
            return;
        }
        DontDestroyOnLoad(gameObject);
    }

    void Start()
    {
        foreach (GameObject objects in carObjectsToDisable)
        {
            objects.SetActive(false);
        }
    }

    public void AddScore(int scoreToAdd)
    {
        currentScore += scoreToAdd;
        currentScore = Mathf.Max(0, currentScore);
        scoreText.text = "Score: " + currentScore; // Update the on-screen score display to reflect the new score after collecting an item
    }

    public void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void HidePlayer()
    {
        foreach (GameObject objects in carObjectsToDisable)
        {
            objects.SetActive(true);
        }
        playerParent.SetActive(false);
    }

    public void HidePlayerCar()
    {
        foreach (GameObject objects in carObjectsToDisable)
        {
            objects.SetActive(false);
        }
        
        GameObject playerCapsule = playerParent.GetComponentInChildren<PlayerScript>().gameObject;
        CharacterController characterController = playerCapsule.GetComponent<CharacterController>();
        characterController.enabled = false;
        playerCapsule.transform.position = carObjectsToDisable[1].transform.parent.GetChild(1).transform.position;
        playerCapsule.transform.Translate(Vector3.left * 3);
        characterController.enabled = true;
        playerParent.SetActive(true);
    }
}
