using UnityEngine;
using UnityEngine.InputSystem;  // Necesario para el Input System

public class DoorInteractor : MonoBehaviour
{
    [Header("=== Settings ===")]

    [Tooltip("The TMP_Text component to display interaction messages.")]
    public TMPro.TMP_Text interactionText;  // Text component for displaying interaction messages
    public string interactionMessage;

    [Header("=== Points Settings ===")]
    [Tooltip("Cost in points to perform the interaction.")]
    public int pointCost = 10;  // Costo en puntos configurable en el inspector

    private bool isPlayerInRange = false;  // Flag to track if the player is in range

    // Referencia al InputAction del sistema de entrada
    private InputAction interactAction;

    private void OnEnable()
    {
        // Obtener el action map Player desde el Input System y asignar la acción Interact
        var playerInputActions = new InputSystem_Actions(); // Asegúrate de que esta clase sea la generada por el Input System
        interactAction = playerInputActions.Player.Interact;  // "Player" es el action map, "Interact" es la acción

        // Habilitar la acción y suscribirse al evento
        interactAction.Enable();  // Habilitar la acción
        interactAction.performed += OnInteractPerformed; // Suscribirse al evento 'performed'
    }

    private void OnDisable()
    {
        // Deshabilitar la acción y eliminar la suscripción al evento
        interactAction.Disable();  // Deshabilitar la acción cuando el objeto se desactiva
        interactAction.performed -= OnInteractPerformed; // Desuscribirse del evento cuando el objeto se desactiva
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entra al trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Mostrar mensaje de interacción
            if (interactionText != null)
            {
                interactionText.text = interactionMessage;  // Mostrar mensaje de interacción
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto que salió del trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Limpiar el mensaje de interacción
            if (interactionText != null)
            {
                interactionText.text = "";  // Vaciar el mensaje cuando el jugador sale del trigger
            }
        }
    }

    // Método que se ejecuta cuando la acción de interactuar es realizada
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Verificar si el jugador está en rango y la acción fue realizada
        if (isPlayerInRange)
        {
            // Obtener el script "Player" para verificar los puntos
            Player player = GameObject.Find("FirstPersonController").GetComponent<Player>();
            if (player == null)
            {
                Debug.LogError("No se encontró el script Player.");
                return;  // Si no se encuentra el script del jugador, salimos de la función
            }

            // Verificar si el jugador tiene suficientes puntos
            if (player.currentPoints >= pointCost)
            {
                // Descontar los puntos por el costo
                player.RemovePoints(pointCost);

                // Remover puerta
                gameObject.SetActive(false);
            }
            else
            {
                // Si no tiene suficientes puntos, mostrar un mensaje
                Debug.Log("No tienes suficientes puntos para realizar la acción.");
            }
        }
    }
}
