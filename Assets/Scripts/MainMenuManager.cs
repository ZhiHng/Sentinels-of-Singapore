/*
* Author: Zhi Hng
* Date: 6 August 2026
* Description: Manages the main menu scene and calls level scenes for gameplay.
*/

using System.Collections;
using TMPro;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    Animator animator;
    [SerializeField] GameObject screenDarken;
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject howToPlayMenu;
    [SerializeField] GameObject creditsMenu;
    [SerializeField] GameObject levelMenu;
    bool isScreenCovered = false;
    Coroutine waitForScreenSwipeCoroutine;
    public static int noOfEnemies = 0;
    public static int civilianSpawnInterval = 0;
    public static int maxCivilians = 0;
    public static int level = 1;
    public static int playTime = 0;
    public static int graphicQuality = 0;
    public static int difficulty = 0;
    [SerializeField] TextMeshProUGUI graphicQualityText;
    [SerializeField] TextMeshProUGUI difficultyText;
    TextMeshProUGUI graphicQualitySubText;
    TextMeshProUGUI difficultySubText;
    public int markerTarget = 0;
    public int menuPage = 0; // 0 = main menu, 1 = settings, 2 = how to play, 3 = credits, 4 = level select
    public int howToPlayPage = 0; // 0 = page 1, 1 = page 2, 2 = page 3
    void Start()
    {
        Cursor.lockState = CursorLockMode.Locked;
        Cursor.visible = false;
        graphicQualitySubText = graphicQualityText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        difficultySubText = difficultyText.transform.GetChild(0).GetComponent<TextMeshProUGUI>();
        animator = GetComponent<Animator>();
        mainMenu.SetActive(true); // 0 Menu
        settingsMenu.SetActive(false); // 1 Menu
        howToPlayMenu.SetActive(false); // 2 Menu
        creditsMenu.SetActive(false); // 3 Menu
        levelMenu.SetActive(false); // 4 Menu
        screenDarken.SetActive(false);
    }
    void OnUp()
    {
        if (waitForScreenSwipeCoroutine != null)
        {
            return;
        }
        markerTarget--;
        GameObject currentMenuObject = null;
        switch (menuPage)
        {
            case 0:
                currentMenuObject = mainMenu;
                break;
            case 1:
                currentMenuObject = settingsMenu;
                break;
            case 2:
                currentMenuObject = howToPlayMenu;
                break;
            case 3:
                currentMenuObject = creditsMenu;
                break;
            case 4:
                currentMenuObject = levelMenu;
                break;
        }
        if (markerTarget > currentMenuObject.transform.childCount - 1)
        {
            markerTarget = 0;
        }
        else if (markerTarget < 0)
        {
            markerTarget = currentMenuObject.transform.childCount - 1;
        }
        animator.SetInteger("markerNumber", markerTarget);
    }
    void OnDown()
    {
        if (waitForScreenSwipeCoroutine != null)
        {
            return;
        }
        markerTarget++;
        GameObject currentMenuObject = null;
        switch (menuPage)
        {
            case 0:
                currentMenuObject = mainMenu;
                break;
            case 1:
                currentMenuObject = settingsMenu;
                break;
            case 2:
                currentMenuObject = howToPlayMenu;
                break;
            case 3:
                currentMenuObject = creditsMenu;
                break;
            case 4:
                currentMenuObject = levelMenu;
                break;
        }
        if (markerTarget > currentMenuObject.transform.childCount - 1)
        {
            markerTarget = 0;
        }
        else if (markerTarget < 0)
        {
            markerTarget = currentMenuObject.transform.childCount - 1;
        }
        animator.SetInteger("markerNumber", markerTarget);
    }
    void OnEnter()
    {
        if (waitForScreenSwipeCoroutine != null)
        {
            return;
        }
        switch (menuPage)
        {
            case 0: // Main Menu
                switch (markerTarget)
                {
                    case 0: // Start Game
                        OpenMenu(levelMenu);
                        menuPage = 4;
                        markerTarget = 0;
                        break;
                    case 1: // Settings
                        OpenMenu(settingsMenu);
                        menuPage = 1;
                        markerTarget = 0;
                        break;
                    case 2: // How to Play
                        OpenMenu(howToPlayMenu);
                        menuPage = 2;
                        markerTarget = 0;
                        howToPlayPage = 0;
                        animator.SetInteger("howToPlayPageNumber", howToPlayPage);
                        break;
                    case 3: // Credits
                        OpenMenu(creditsMenu);
                        menuPage = 3;
                        markerTarget = 0;
                        break;
                    case 4: // Quit Game
                        QuitGame();
                        break;
                }
                animator.SetTrigger("screenSwipe");
                break;


            case 1: // Settings Menu
                switch (markerTarget)
                {
                    case 0: // Graphic Quality
                        graphicQuality++;
                        if (graphicQuality > 2)
                        {
                            graphicQuality = 0;
                        }
                        UpdateQualityText();
                        break;
                    case 1: // Difficulty
                        difficulty++;
                        if (difficulty > 2)
                        {
                            difficulty = 0;
                        }
                        UpdateDifficultyText();
                        break;
                    case 2: // Back to Main Menu
                        OpenMenu(mainMenu);
                        menuPage = 0;
                        markerTarget = 0;
                        animator.SetTrigger("screenSwipe");
                        break;
                }
                break;


            case 2: // How to Play Menu
                switch (markerTarget)
                {
                    case 0: // Previous Page
                        howToPlayPage--;
                        if (howToPlayPage < 0)
                        {
                            howToPlayPage = howToPlayMenu.transform.GetChild(2).GetChild(1).childCount - 1;
                        }
                        // Insert animation for moving page
                        animator.SetInteger("howToPlayPageNumber", howToPlayPage);
                        break;

                    case 1: // Next Page
                        howToPlayPage++;
                        if (howToPlayPage > howToPlayMenu.transform.GetChild(2).GetChild(1).childCount - 1)
                        {
                            howToPlayPage = 0;
                        }
                        // Insert animation for moving page
                        animator.SetInteger("howToPlayPageNumber", howToPlayPage);
                        break;
                        
                    case 2: // Back to Main Menu
                        OpenMenu(mainMenu);
                        menuPage = 0;
                        markerTarget = 0;
                        animator.SetTrigger("screenSwipe");
                        break;
                }
                break;


            case 3: // Credits Menu
                switch (markerTarget)
                {
                    case 0: // Back to Main Menu
                        OpenMenu(mainMenu);
                        menuPage = 0;
                        markerTarget = 0;
                        animator.SetTrigger("screenSwipe");
                        break;
                }
                break;


            case 4: // Level Select Menu
                switch (markerTarget)
                {
                    case 0: // Level One
                        level = 1;

                        StartLevel();
                        break;
                    case 1: // Level Two
                        level = 2;
                        StartLevel();
                        break;
                    case 2: // Level Three
                        level = 3;
                        StartLevel();
                        break;
                    case 3: // Back to Main Menu
                        OpenMenu(mainMenu);
                        menuPage = 0;
                        markerTarget = 0;
                        animator.SetTrigger("screenSwipe");
                        break;

                }
                break;
        }
    }
    void OnEsc()
    {
        switch (menuPage)
        {
            case 0: // Main Menu
                break;
            case 1: // Settings Menu
                OpenMenu(mainMenu);
                menuPage = 0;
                markerTarget = 0;
                animator.SetTrigger("screenSwipe");
                break;
            case 2: // How to Play Menu
                OpenMenu(mainMenu);
                menuPage = 0;
                markerTarget = 0;
                animator.SetTrigger("screenSwipe");
                break;
            case 3: // Credits Menu
                OpenMenu(mainMenu);
                menuPage = 0;
                markerTarget = 0;
                animator.SetTrigger("screenSwipe");
                break;
            case 4: // Level Select Menu
                OpenMenu(mainMenu);
                menuPage = 0;
                markerTarget = 0;
                animator.SetTrigger("screenSwipe");
                break;
        }
    }
    void StartLevel()
    {
        // Set level parameters here.
        switch (level)
        {
            case 1:
                noOfEnemies = 10;
                civilianSpawnInterval = 3;
                maxCivilians = 10;
                playTime = 3;
                break;
            case 2:
                noOfEnemies = 20;
                civilianSpawnInterval = 2;
                maxCivilians = 15;
                playTime = 4;
                break;
            case 3:
                noOfEnemies = 30;
                civilianSpawnInterval = 1;
                maxCivilians = 20;
                playTime = 5;
                break;
        }
        ChangeScene("S.O.S Game");
    }
    void OpenMenu(GameObject menuToOpen)
    {
        waitForScreenSwipeCoroutine = StartCoroutine(WaitForScreenSwipe(menuToOpen));
    }
    IEnumerator WaitForScreenSwipe(GameObject menuToOpen)
    {
        yield return new WaitUntil(() => isScreenCovered);
        isScreenCovered = false;
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        howToPlayMenu.SetActive(false);
        creditsMenu.SetActive(false);
        levelMenu.SetActive(false);

        menuToOpen.SetActive(true);
        if (menuToOpen == mainMenu)
        {
            screenDarken.SetActive(false);
        }
        else
        {
            screenDarken.SetActive(true);
        }
        animator.SetInteger("markerNumber", markerTarget);
        animator.SetInteger("menuNumber", menuPage);
        animator.SetTrigger("switchMenu");

        waitForScreenSwipeCoroutine = null;
    }
    void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    void QuitGame()
    {
        Application.Quit();
    }

    void ScreenCovered()
    {
        isScreenCovered = true;
    }
    void UpdateQualityText()
    {
        switch (graphicQuality)
        {
            case 0:
                graphicQualityText.text = "Quality: Low";
                graphicQualitySubText.text = "No shadows casted\nHalf texture quality";
                break;
            case 1:
                graphicQualityText.text = "Quality: Medium";
                graphicQualitySubText.text = "Normal texture quality";
                break;
            case 2:
                graphicQualityText.text = "Quality: High";
                graphicQualitySubText.text = "Normal texture quality\nShadows casted";
                break;
        }
    }
    void UpdateDifficultyText()
    {
        switch (difficulty)
        {
            case 0:
                difficultyText.text = "Difficulty: Easy";
                difficultySubText.text = "Great for rookie enforcers trying to get the hang of keeping Singapore safe.\nMistakes are forgiving.";
                break;
            case 1:
                difficultyText.text = "Difficulty: Normal";
                difficultySubText.text = "A good challenge for experienced enforcers.\nMistakes are penalized.";
                break;
            case 2:
                difficultyText.text = "Difficulty: Hard";
                difficultySubText.text = "Experienced enforcers are expected to make no mistakes.\nMistakes are severely punished.";
                break;
        }
    }
}
