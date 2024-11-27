using UnityEngine;
using UnityEngine.InputSystem;  // Necesario para el Input System

public class WallWeaponInteractor : MonoBehaviour
{
    [Header("=== Settings ===")]
    [Tooltip("The integer value to pass to the 'OverwriteCurrentWeapon' function in the 'WeaponInventory' script.")]
    public int weaponIndex = 0;  // Integer to modify in the inspector

    [Tooltip("The TMP_Text component to display interaction messages.")]
    public TMPro.TMP_Text interactionText;  // Text component for displaying interaction messages
    public string defaultMessage = "";  // Mensaje por defecto

    [Header("=== Points Settings ===")]
    [Tooltip("Cost in points to perform the interaction.")]
    public int pointCost = 10;  // Costo en puntos configurable en el inspector

    private bool isPlayerInRange = false;  // Flag to track if the player is in range

    // Referencia al InputAction del sistema de entrada
    private InputAction interactAction;

    Player player;
    WeaponInventory weaponInventory;

    private void Start()
    {
        player = GameObject.Find("FirstPersonController").GetComponent<Player>();
        weaponInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponInventory>();
    }
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

            // Actualizar el mensaje de interacción dependiendo de la situación
            if (interactionText != null)
            {
                if (weaponInventory != null && player != null)
                {
                    // Obtener el ID del arma equipada
                    int currentID = weaponInventory.GetCurrentWeaponID();

                    // Verificar si el ID del arma equipada coincide con el 'weaponIndex'
                    if (currentID == weaponIndex)
                    {
                        // Si el jugador tiene el arma, mostrar el mensaje de compra de munición
                        int refillCost = pointCost / 2;
                        if (player.currentPoints >= refillCost)
                        {
                            interactionText.text = "Comprar munición: " + refillCost + " puntos";
                        }
                        else
                        {
                            interactionText.text = "No tienes suficientes puntos para munición.";
                        }
                    }
                    else
                    {
                        // Si el jugador no tiene el arma, mostrar el mensaje de compra del arma
                        if (player.currentPoints >= pointCost)
                        {
                            interactionText.text = "Comprar arma: " + pointCost + " puntos";
                        }
                        else
                        {
                            interactionText.text = "No tienes suficientes puntos para el arma.";
                        }
                    }
                }
            }
        }
    }

    private void OnTriggerExit(Collider other)
    {
        // Verificar si el objeto que salió del trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Solo cambiar el mensaje si ya no hay interacción posible
            if (interactionText != null)
            {
                interactionText.text = defaultMessage;  // Restaurar el mensaje por defecto
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
            if (player == null)
            {
                Debug.LogError("No se encontró el script Player.");
                return;  // Si no se encuentra el script del jugador, salimos de la función
            }

            // Obtener el script "WeaponInventory" del objeto con tag "Player"
            if (weaponInventory == null)
            {
                Debug.Log("No se encontró el inventory");
                return;  // Si no se encuentra el inventario, salimos de la función
            }

            // Obtener el ID del arma equipada
            int currentID = weaponInventory.GetCurrentWeaponID();

            // Verificar si el ID del arma equipada coincide con el 'weaponIndex'
            if (currentID == weaponIndex)
            {
                // El jugador ya tiene el arma, ofrecer recargar munición a la mitad del precio original
                int refillCost = pointCost / 2;

                if (player.currentPoints >= refillCost)
                {
                    // Descontar los puntos para la recarga de munición
                    player.RemovePoints(refillCost);

                    // Llamar a la función de recarga de munición
                    PlayerShooting playerShooting = GameObject.Find("Weapons").GetComponent<PlayerShooting>();
                    if (playerShooting != null)
                    {
                        playerShooting.RefillAmmo();
                    }
                    else
                    {
                        Debug.LogWarning("PlayerShooting no se encontró");
                        return;
                    }

                    // Cambiar el mensaje de interacción después de comprar
                    Debug.Log("Munición recargada por la mitad del precio.");
                }
                else
                {
                    // Si no tiene suficientes puntos para recargar munición
                    Debug.Log("No tienes suficientes puntos para recargar munición.");
                }
            }
            else
            {
                // El jugador no tiene el arma, permitir la compra del arma
                if (player.currentPoints >= pointCost)
                {
                    // Descontar los puntos por el costo
                    player.RemovePoints(pointCost);

                    // Llamar a la función "OverwriteCurrentWeapon" en el script "WeaponInventory" con el índice configurado
                    weaponInventory.OverwriteCurrentWeapon(weaponIndex);
                    PlayerShooting playerShooting = GameObject.Find("Weapons").GetComponent<PlayerShooting>();
                    playerShooting.ApplyRotationShake();

                    // Cambiar el mensaje de interacción después de comprar
                    if (interactionText != null)
                    {
                        interactionText.text = "Compraste el arma por " + pointCost + " puntos";
                    }

                    Debug.Log("Arma comprada.");
                }
                else
                {
                    // Si no tiene suficientes puntos para comprar el arma
                    Debug.Log("No tienes suficientes puntos para comprar el arma.");
                }
            }
        }
    }
}
