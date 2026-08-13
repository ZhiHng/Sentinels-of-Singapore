/*
* Author: Zhi Hng
* Date: 13 August 2026
* Description: Handles management between scenes and player score.
*/

using System.Collections;
using Ezereal;
using TMPro;
using Unity.Cinemachine;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;

public class GameManager : MonoBehaviour
{
    Animator animator;
    public static GameManager Instance;
    int currentScore;
    GameObject playerParent;
    [SerializeField] TextMeshProUGUI scoreText; // Reference to the UI text element that displays the player's score
    [SerializeField] TextMeshProUGUI timerText; 
    [SerializeField] TextMeshProUGUI informationUI;
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
    [SerializeField] GameObject walkieTalkie;
    AudioSource walkieTalkieAudio;
    Coroutine stationCallCoroutine;
    Coroutine showTextCoroutine;
    [SerializeField] float stationCallInterval;
    bool isStationCallAccept = false;
    bool isStationCallReact = false;

    public float typingSpeed = 0.05f; // Delay between each character
    [SerializeField] GameObject beaconPrefab;
    GameObject trackingObject;
    bool[] popUpsTracking = new bool[8]; // 0: Pickpocket, 1: Smoker, 2: Fighter, 3: Crashing, 4: Knocking over people, 5: Jaywalking, 6: Driving off road, 7: Red Light Driving
    bool carFirstSpawn;

    void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            scoreText.text = "Score: " + 0;
            timerText.text = "Shift Ends In...";
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
        walkieTalkie.SetActive(false);
        walkieTalkieAudio = walkieTalkie.GetComponent<AudioSource>();
    }

    public void AddScore(int scoreToAdd)
    {
        if (scoreToAdd > 0)
        {
            currentScore += scoreToAdd;
        }
        else
        {
            currentScore += scoreToAdd * (MainMenuManager.difficulty + 1);
        }
        
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
        PlayerCarScript carScript = carObjectsToDisable[0].GetComponent<PlayerCarScript>();
        carScript.gameObject.GetComponent<EzerealSoundController>().TurnOnEngineSound();
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
        PlayerCarScript carScript = carObjectsToDisable[0].GetComponent<PlayerCarScript>();
        carScript.gameObject.GetComponent<EzerealSoundController>().TurnOffEngineSound();
        playerCapsule.transform.position = carScript.carBodyTransform.position + -carScript.carBodyTransform.right * 3;
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
        walkieTalkie.SetActive(false);
        if (stationCallCoroutine != null)
        StopCoroutine(stationCallCoroutine);
        if (!playerParent.activeSelf)
        {
            HidePlayerCar();
        }
        playerParent.GetComponentInChildren<CharacterController>().enabled = false;
        StartCoroutine(EndGameAnimations(maxPossibleScore));
    }
    public void ResetToMainMenu(int level) // -1 level if return by pause menu
    {
        
        if (stationCallCoroutine != null)
        {
            StopCoroutine(stationCallCoroutine);
            stationCallCoroutine = null;
        }
        isStationCallAccept = false;
        isStationCallReact = false;
        walkieTalkie.SetActive(false);
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
        timerText.gameObject.SetActive(false);
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
        timerText.gameObject.SetActive(true);
        endScreenUI.SetActive(false);
        pauseScreen.SetActive(false);
        isPauseMenu = false;
        Time.timeScale = 1;
        currentScore = 0;
        scoreText.text = "Score: " + currentScore;
        timerText.text = "Shift Ends In...";
        stationCallCoroutine = StartCoroutine(StationCall());
    }
    public void EscPressed()
    {
        if (isEndScreen)
        {
            return;
        }
        if (!isPauseMenu)
        {
            Time.timeScale = 0;
            pauseScreen.SetActive(true);
            isPauseMenu = true;
            playerParent.GetComponentInChildren<PlayerScript>().isResume = true;
            carObjectsToDisable[0].GetComponent<PlayerCarScript>().isResume = true;
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
    IEnumerator StationCall()
    {
        while (true)
        {
            isStationCallAccept = false;
            isStationCallReact = false;
            yield return new WaitForSeconds(Random.Range(stationCallInterval - 2, stationCallInterval + 2));
            yield return new WaitUntil(() => NPCManager.offenders.Count > 0);
            walkieTalkie.SetActive(true);
            animator.SetBool("isStationCall", true);
            walkieTalkieAudio.Play();
            yield return new WaitUntil(() => isStationCallReact == true);
            
            if (isStationCallAccept == true && NPCManager.offenders.Count > 0)
            {
                SpawnBeaconOnGameObject(NPCManager.offenders[Random.Range(0, NPCManager.offenders.Count)]);
            }
            else if (!(NPCManager.offenders.Count > 0))
            {
                BroadcastMessage("Suspect has escaped while responding");
            }
            animator.SetBool("isStationCall", false);
            yield return new WaitForSeconds(2);
            walkieTalkie.SetActive(false);
            yield return new WaitUntil(() => trackingObject == null);
        }
    }

    public void Clicked1()
    {
        isStationCallAccept = true;
        isStationCallReact = true;
    }

    public void Clicked2()
    {
        isStationCallReact = true;
        isStationCallAccept = false;
    }

    void SpawnBeaconOnGameObject(GameObject gameObject)
    {
        GameObject spawnBeacon = Instantiate(beaconPrefab, gameObject.transform.position, Quaternion.identity);
        spawnBeacon.GetComponent<BeaconScript>().LinkToGameObject(gameObject);
        trackingObject = gameObject;
    }

    public void CheckTracking(GameObject gameObject)
    {
        if (gameObject == trackingObject)
        {
            AddScore(20);
            print("add score");
        }
    }
    public void BroadcastMessage(string message = "", int preset = -1) // 0: Pickpocket, 1: Smoker, 2: Fighter, 3: Crashing, 4: Knocking over people, 5: Jaywalking, 6: Driving off road, 7: Red Light Driving
    {
        string finalMessage = message;
        if (preset >= 0)
        {
            if (popUpsTracking[preset] != true)
            {
                switch (preset)
                {
                    case 0:
                        finalMessage = "Pickpockets can be sentenced up to 3 years in jail or fined or both";
                        break;
                    case 1:
                        finalMessage = "Vapers can be fined up to $10,000";
                        break;
                    case 2:
                        finalMessage = "Public Affray can be sentenced up to 1 years in jail or fined up to $5,000 or both";
                        break;
                    case 3:
                        finalMessage = "Infrastructure damage can be fined up to $5,000 and possible jail if reckless driving";
                        break;
                    case 4:
                        finalMessage = "Knocking over people can be sentenced up to 2 years in jail if only injury and up to 10 years if it is fatal, including fines";
                        break;
                    case 5:
                        finalMessage = "Jaywalking can be fined up to $100";
                        break;
                    case 6:
                        finalMessage = "Driving off roads onto pedestrian areas can be fined up to $5,000 and/or up to 1 year in jail";
                        if (carFirstSpawn != true)
                        {
                            carFirstSpawn = true;
                            return;
                        }
                        break;
                    case 7:
                        finalMessage = "Driving a red light can be fined up to $200";
                        break;
                }
                popUpsTracking[preset] = true;
            }
        }
        if (finalMessage == "") return;
        if (showTextCoroutine != null)
        {
            StopCoroutine(showTextCoroutine);
        }
        showTextCoroutine = StartCoroutine(ShowText(5f, finalMessage));
    }
    /// <summary>
    /// Enables the TextMeshProUGUI for a duration to show a message and disables it again
    /// </summary>
    /// <param name="duration">Duration for the message to stay on the screen</param>
    /// <param name="message">Message to be shown</param>
    /// <returns></returns>
    IEnumerator ShowText(float duration, string message)
    {
        RectTransform background = informationUI.transform.parent as RectTransform;
        informationUI.text = message;
        background.anchoredPosition = new Vector2(0, 260);
        informationUI.transform.parent.gameObject.SetActive(true); // Show the text
        float elapsed = 0f;
        float moveInNOutDuration = 1;
        Vector2 pos = background.anchoredPosition;

        while (elapsed < moveInNOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveInNOutDuration;
            pos.y = Mathf.Lerp(260, 180, t);
            background.anchoredPosition = pos;
            yield return null;
        }

        // Ensure final position
        pos.y = 180;
        background.anchoredPosition = pos;

        yield return new WaitForSeconds(duration);

        elapsed = 0;
        while (elapsed < moveInNOutDuration)
        {
            elapsed += Time.deltaTime;
            float t = elapsed / moveInNOutDuration;
            pos.y = Mathf.Lerp(180, 260, t);
            background.anchoredPosition = pos;
            yield return null;
        }
        informationUI.transform.parent.gameObject.SetActive(false); //Hide the text
    }
    public void UpdateTimer(int timeLeft)
    {
        timerText.text = timeLeft.ToString();
    }
}
