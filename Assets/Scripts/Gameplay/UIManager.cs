using UnityEngine;
using UnityEngine.InputSystem;

public class UIManager : MonoBehaviour
{
    [Header("UI Panels")]
    public GameObject pauseMenu;
    public GameObject settingsMenu;
    public GameObject hudPanel;
    [Header("References")]
    public GameManager gameManager;
    public PlayerInput playerInput;
    private bool isMenuOpen = false;
    private bool isPaused = false;
    void Start()
    {
        if (gameManager == null)
            gameManager = FindFirstObjectByType<GameManager>();
        if (playerInput == null)
            playerInput = FindFirstObjectByType<PlayerInput>();
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (settingsMenu != null)
            settingsMenu.SetActive(false);
        if (gameManager != null)
            gameManager.SetGameplayCursor();
    }
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.Escape))
        {
            TogglePauseMenu();
        }
    }
    public void TogglePauseMenu()
    {
        if (gameManager != null && !gameManager.IsGameActive())
            return;
        isMenuOpen = !isMenuOpen;
        if (pauseMenu != null)
            pauseMenu.SetActive(isMenuOpen);
        if (hudPanel != null)
            hudPanel.SetActive(!isMenuOpen);
        if (gameManager != null)
        {
            if (isMenuOpen)
            {
                gameManager.SetUICursor();
                Time.timeScale = 0f;
                isPaused = true;
            }
            else
            {
                gameManager.SetGameplayCursor();
                Time.timeScale = 1f;
                isPaused = false;
            }
        }
        else if (playerInput != null)
        {
            if (isMenuOpen)
            {
                Cursor.lockState = CursorLockMode.None;
                Cursor.visible = true;
                playerInput.SwitchCurrentActionMap("UI");
                Time.timeScale = 0f;
                isPaused = true;
            }
            else
            {
                Cursor.lockState = CursorLockMode.Locked;
                Cursor.visible = false;
                playerInput.SwitchCurrentActionMap("Gameplay");
                Time.timeScale = 1f;
                isPaused = false;
            }
        }
    }
    public void ResumeGame()
    {
        if (isMenuOpen)
        {
            TogglePauseMenu();
        }
    }
    public void OpenSettings()
    {
        if (pauseMenu != null)
            pauseMenu.SetActive(false);
        if (settingsMenu != null)
            settingsMenu.SetActive(true);
    }
    public void CloseSettings()
    {
        if (settingsMenu != null)
            settingsMenu.SetActive(false);
        if (pauseMenu != null)
            pauseMenu.SetActive(true);
    }
    public void QuitToMainMenu()
    {
        Time.timeScale = 1f;
        Debug.Log("Quit to Main Menu - implement scene loading here");
    }
    public void QuitGame()
    {
        Time.timeScale = 1f;
        Application.Quit();
    }
    public bool IsMenuOpen() { return isMenuOpen; }
    public bool IsPaused() { return isPaused; }
}
