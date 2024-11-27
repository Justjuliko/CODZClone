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
        // Obtener el action map Player desde el Input System y asignar la acci�n Interact
        var playerInputActions = new InputSystem_Actions(); // Aseg�rate de que esta clase sea la generada por el Input System
        interactAction = playerInputActions.Player.Interact;  // "Player" es el action map, "Interact" es la acci�n

        // Habilitar la acci�n y suscribirse al evento
        interactAction.Enable();  // Habilitar la acci�n
        interactAction.performed += OnInteractPerformed; // Suscribirse al evento 'performed'
    }

    private void OnDisable()
    {
        // Deshabilitar la acci�n y eliminar la suscripci�n al evento
        interactAction.Disable();  // Deshabilitar la acci�n cuando el objeto se desactiva
        interactAction.performed -= OnInteractPerformed; // Desuscribirse del evento cuando el objeto se desactiva
    }

    private void OnTriggerEnter(Collider other)
    {
        // Verificar si el objeto que entra al trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;
            // Mostrar mensaje de interacci�n
            if (interactionText != null)
            {
                interactionText.text = interactionMessage;  // Mostrar mensaje de interacci�n
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto que sali� del trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;
            // Limpiar el mensaje de interacci�n
            if (interactionText != null)
            {
                interactionText.text = "";  // Vaciar el mensaje cuando el jugador sale del trigger
            }
        }
    }

    // M�todo que se ejecuta cuando la acci�n de interactuar es realizada
    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        // Verificar si el jugador est� en rango y la acci�n fue realizada
        if (isPlayerInRange)
        {
            // Obtener el script "Player" para verificar los puntos
            Player player = GameObject.Find("FirstPersonController").GetComponent<Player>();
            if (player == null)
            {
                Debug.LogError("No se encontr� el script Player.");
                return;  // Si no se encuentra el script del jugador, salimos de la funci�n
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
                Debug.Log("No tienes suficientes puntos para realizar la acci�n.");
            }
        }
    }
}
