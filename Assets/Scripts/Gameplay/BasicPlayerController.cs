using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.InputSystem;
public class BasicPlayerController : MonoBehaviour
{
    [Header("Other")]
    public GameObject Menu;
    private bool menuActive = false;
    public PlayerInput _playerInput;

    public PlayerMovement playerMovement;
    public GunController gunController;

    private void Awake()
    {
        if (playerMovement == null) playerMovement = GetComponent<PlayerMovement>();
        if (gunController == null) gunController = GetComponent<GunController>();

        Menu.SetActive(false);
        menuActive = false;
    }
    void Update()
    {
    }
    public void OnMenu(InputAction.CallbackContext context)
    {
        if (context.performed)
        {
            ToggleMenu();
        }
    }
    public void OnCancel(InputAction.CallbackContext context)
    {
        if (context.performed && menuActive)
        {
            ToggleMenu();
        }
    }

    public void ToggleMenu()
    {
        if (Menu != null)
        {
            menuActive = !menuActive;
            Menu.SetActive(menuActive);
            Debug.Log($"Menu toggled to: {menuActive}");

            GameManager gameManager = FindFirstObjectByType<GameManager>();
            if (gameManager != null)
            {
                if (menuActive)
                {
                    gameManager.SetUICursor();
                    Time.timeScale = 0f;
                }
                else
                {
                    gameManager.SetGameplayCursor();
                    Time.timeScale = 1f;
                }
            }
            else
            {
                if (menuActive)
                {
                    Cursor.lockState = CursorLockMode.None;
                    Cursor.visible = true;
                    Time.timeScale = 0f;
                }
                else
                {
                    Cursor.lockState = CursorLockMode.Locked;
                    Cursor.visible = false;
                    Time.timeScale = 1f;
                }
            }
        }
    }

    public void Quit()
    {
        Application.Quit();
    }
}
