/*
* Author: Zhi Hng
* Date: 2 August 2026
* Description: Manages the main menu scene and calls level scenes for gameplay.
*/

using UnityEngine;

public class MainMenuManager : MonoBehaviour
{
    // dictionary
    [SerializeField] GameObject mainMenu;
    [SerializeField] GameObject settingsMenu;
    [SerializeField] GameObject howToPlayMenu;
    [SerializeField] GameObject creditsMenu;
    [SerializeField] GameObject levelMenu;
    string previousMenu;
    public static int noOfEnemies = 0;
    public static int civilianSpawnInterval = 0;
    public static int maxCivilians = 0;
    public static int playTime = 0;
    void Start()
    {
        mainMenu.SetActive(true);
        settingsMenu.SetActive(false);
        howToPlayMenu.SetActive(false);
        creditsMenu.SetActive(false);
        levelMenu.SetActive(false);
    }
    public void StartLevel()
    {
        // Set level parameters here.
        noOfEnemies = 10;
        civilianSpawnInterval = 3;
        maxCivilians = 10;
        playTime = 2;
        ChangeScene("S.O.S Game");
    }
    public void OpenMenu(GameObject menuToOpen)
    {
        mainMenu.SetActive(false);
        settingsMenu.SetActive(false);
        howToPlayMenu.SetActive(false);
        creditsMenu.SetActive(false);
        levelMenu.SetActive(false);

        menuToOpen.SetActive(true);
    }
    public void ChangeScene(string sceneName)
    {
        UnityEngine.SceneManagement.SceneManager.LoadScene(sceneName);
    }

    public void QuitGame()
    {
        Application.Quit();
    }
}
