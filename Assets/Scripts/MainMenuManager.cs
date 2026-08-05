/*
* Author: Zhi Hng
* Date: 5 August 2026
* Description: Manages the main menu scene and calls level scenes for gameplay.
*/

using System.Collections;
using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    Animator animator;
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
    public int markerTarget = 0;
    public int menuPage = 0; // 0 = main menu, 1 = settings, 2 = how to play, 3 = credits, 4 = level select
    void Start()
    {
        animator = GetComponent<Animator>();
        mainMenu.SetActive(true); // 0 Menu
        settingsMenu.SetActive(false); // 1 Menu
        howToPlayMenu.SetActive(false); // 2 Menu
        creditsMenu.SetActive(false); // 3 Menu
        levelMenu.SetActive(false); // 4 Menu
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
                        break;
                    case 1: // Difficulty
                        difficulty++;
                        if (difficulty > 2)
                        {
                            difficulty = 0;
                        }
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
                    case 0: // Back to Main Menu
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
        noOfEnemies = 10;
        civilianSpawnInterval = 3;
        maxCivilians = 10;
        playTime = 2;
        ChangeScene("S.O.S Game");
    }
    void OpenMenu(GameObject menuToOpen)
    {
        StartCoroutine(WaitForScreenSwipe(menuToOpen));
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
        animator.SetInteger("markerNumber", markerTarget);
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
}
