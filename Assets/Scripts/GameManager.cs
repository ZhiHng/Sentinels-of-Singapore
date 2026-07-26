/*
* Author: Zhi Hng
* Date: 26 July 2026
* Description: Handles management between scenes and player score.
*/

using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance;
    int currentScore;

    [SerializeField]
    TextMeshProUGUI scoreText; // Reference to the UI text element that displays the player's score

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
}
