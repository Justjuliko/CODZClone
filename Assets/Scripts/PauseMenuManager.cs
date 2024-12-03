using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu Settings")]
    public GameObject pauseMenu; // Reference to the pause menu
    private bool isPaused = false; // Game state (paused or not)

    private InputAction pauseAction; // Pause action from the Input System

    private void Awake()
    {
        var playerInputActions = new InputSystem_Actions(); // Initialize the actions
        pauseAction = playerInputActions.Player.Pause; // Assign the pause action
        pauseAction.Enable(); // Enable the action
    }

    private void OnEnable()
    {
        pauseAction.performed += TogglePause; // Link the action to the method
    }

    private void OnDisable()
    {
        pauseAction.performed -= TogglePause; // Unlink the action
    }

    private void TogglePause(InputAction.CallbackContext context)
    {
        isPaused = !isPaused; // Toggle the pause state

        if (isPaused)
        {
            PauseGame();
        }
        else
        {
            ResumeGame();
        }
    }

    private void PauseGame()
    {
        Time.timeScale = 0f; // Pause time
        pauseMenu.SetActive(true); // Show the pause menu
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // Resume time
        pauseMenu.SetActive(false); // Hide the pause menu
    }
}
