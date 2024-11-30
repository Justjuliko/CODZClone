using UnityEngine;
using UnityEngine.InputSystem;

public class PauseMenuManager : MonoBehaviour
{
    [Header("Pause Menu Settings")]
    public GameObject pauseMenu; // Referencia al menú de pausa
    private bool isPaused = false; // Estado del juego (pausado o no)

    private InputAction pauseAction; // Acción de pausa del Input System

    private void Awake()
    {
        var playerInputActions = new InputSystem_Actions(); // Inicializar las acciones
        pauseAction = playerInputActions.Player.Pause; // Asignar la acción de pausa
        pauseAction.Enable(); // Activar la acción
    }

    private void OnEnable()
    {
        pauseAction.performed += TogglePause; // Vincular la acción al método
    }

    private void OnDisable()
    {
        pauseAction.performed -= TogglePause; // Desvincular la acción
    }

    private void TogglePause(InputAction.CallbackContext context)
    {
        isPaused = !isPaused; // Alternar el estado de pausa

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
        Time.timeScale = 0f; // Pausar el tiempo
        pauseMenu.SetActive(true); // Mostrar el menú de pausa
    }

    private void ResumeGame()
    {
        Time.timeScale = 1f; // Reanudar el tiempo
        pauseMenu.SetActive(false); // Ocultar el menú de pausa
    }
}
