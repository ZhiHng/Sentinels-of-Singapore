/*
* Author: Zhi Hng
* Date: 8 August 2026
* Description: Handles management between scenes and player score.
*/

using System.Collections;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    Animator animator;
    public static GameManager Instance;
    int currentScore;
    GameObject playerParent;
    [SerializeField] TextMeshProUGUI scoreText; // Reference to the UI text element that displays the player's score
    GameObject[] carObjectsToDisable = new GameObject[3];
    [SerializeField] GameObject endScreenUI;
    [SerializeField] GameObject crosshair;
    [SerializeField] GameObject pauseScreen;
    TextMeshProUGUI[] endScreenText;
    CinemachineBrain playerCinemachineBrain;
    Volume globalVolume; // Uses global volume post processing to change screen colour and vignette
    ColorAdjustments colorAdjustments;
    Coroutine typeTextCoroutine;
    public int[] levelScores = new int[3];
    public string[] levelGrades = new string[3];
    public bool isEndScreen = false;
    string grade = "";
    [HideInInspector] public bool isPauseMenu = false;

    public float typingSpeed = 0.05f; // Delay between each character
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
        animator = GetComponent<Animator>();
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


    void ShowText(string message, TextMeshProUGUI uiText)
    {
        typeTextCoroutine = StartCoroutine(TypeText(message, uiText));
    }

    IEnumerator TypeText(string message, TextMeshProUGUI uiText)
    {
        uiText.text = ""; // Clear text first
        foreach (char c in message)
        {
            uiText.text += c; // Add one character at a time
            yield return new WaitForSeconds(typingSpeed);
        }
        typeTextCoroutine = null;
    }

    IEnumerator EndGameAnimations(int maxPossibleScore)
    {
        playerCinemachineBrain.enabled = false;
        GameObject[] uiInScene = GameObject.FindGameObjectsWithTag("UI");
        foreach (GameObject uiText in uiInScene)
        {
            uiText.SetActive(false);
        }

        //Controls the gradual darken of the screen
        float tick = 0f;
        Transform playerCamera = playerParent.transform.GetChild(0);
        float rotationSpeed = 0.01f;
        colorAdjustments.active = true;
        while (tick < 1f)
        {
            tick += Time.deltaTime / 3f; // 3 Seconds
            colorAdjustments.colorFilter.value = Color.Lerp(Color.white, Color.black, tick); // gradually darken
            playerCamera.transform.Rotate(Vector3.left * rotationSpeed * tick);
            yield return null;
        }
        
        isEndScreen = true;

        endScreenUI.SetActive(true);
        foreach(TextMeshProUGUI text in endScreenText) // Remove all text
        {
            text.text = "";
        }
        for (int i = 0; i < endScreenText.Length; i++)
        {
            yield return new WaitUntil(() => typeTextCoroutine == null);
            switch (i)
            {
                case 0:
                    ShowText("Shift Over", endScreenText[i]);
                    break;
                case 1:
                    ShowText("Score\n" + currentScore + " / " + maxPossibleScore, endScreenText[i]);
                    break;
                case 2:
                    float ratingNumber = currentScore/maxPossibleScore;
                    grade = "";
                    if (ratingNumber < 0.33)
                    {
                        grade = "Mediocre";
                    }
                    else if (ratingNumber < 0.66)
                    {
                        grade = "Great";
                    }
                    else if (ratingNumber < 0.9)
                    {
                        grade = "Excellent";
                    }
                    else
                    {
                        grade = "Perfect";
                    }
                    ShowText("Rating\n" + grade, endScreenText[i]);
                    break;
                case 3:
                    ShowText("Press Enter or Space", endScreenText[i]);
                    break;
            }
        }
    }
    public void EndGame(int maxPossibleScore)
    {
        if (!playerParent.activeSelf)
        {
            HidePlayerCar();
        }
        playerParent.GetComponentInChildren<CharacterController>().enabled = false;
        StartCoroutine(EndGameAnimations(maxPossibleScore));
    }
    public void ResetToMainMenu(int level) // -1 level if return by pause menu
    {
        if (level != -1)
        {
            NPCManager.spawnedCivilians.Clear();
            if (levelScores[level] <= currentScore)
            {
                levelScores[level] = currentScore;
                levelGrades[level] = grade;
            }
        }
        
        currentScore = 0;
        crosshair.SetActive(false);
        scoreText.gameObject.SetActive(false);
        endScreenUI.SetActive(false);
        pauseScreen.SetActive(false);
        isPauseMenu = false;
        Time.timeScale = 1;
        UnityEngine.SceneManagement.SceneManager.LoadScene("Main Menu");
    }
    public void ResetToGameState()
    {
        playerParent = GameObject.FindGameObjectWithTag("Player Parent");
        playerCinemachineBrain = playerParent.GetComponentInChildren<CinemachineBrain>();
        globalVolume = GameObject.FindGameObjectWithTag("Global Volume").GetComponent<Volume>();
        endScreenText = endScreenUI.GetComponentsInChildren<TextMeshProUGUI>();
        globalVolume.profile.TryGet<ColorAdjustments>(out colorAdjustments); // Get reference to colour adjustments
        GameObject playerCarParent = GameObject.FindGameObjectWithTag("Player Car Parent");
        carObjectsToDisable[0] = playerCarParent.transform.GetChild(0).gameObject;
        carObjectsToDisable[1] = playerCarParent.transform.GetChild(2).gameObject;
        carObjectsToDisable[2] = playerCarParent.transform.GetChild(3).gameObject;
        foreach (GameObject objects in carObjectsToDisable)
        {
            objects.SetActive(false);
        }

        crosshair.SetActive(true);
        scoreText.gameObject.SetActive(true);
        endScreenUI.SetActive(false);
        pauseScreen.SetActive(false);
        isPauseMenu = false;
        Time.timeScale = 1;
        currentScore = 0;
        scoreText.text = "Score: " + currentScore;
    }
    public void EscPressed()
    {
        if (!isPauseMenu)
        {
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
            isPauseMenu = true;
            playerParent.GetComponentInChildren<PlayerScript>().isResume = true;
            animator.SetBool("isResume", true);
        }
        else
        {
            Time.timeScale = 1;
            pauseScreen.SetActive(false);
            isPauseMenu = false;
        }
    }
    public void ChangePauseMenuScroll(bool isResume)
    {
        animator.SetBool("isResume", isResume);
    }
}
