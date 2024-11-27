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

            // Actualizar el mensaje de interacci�n dependiendo de la situaci�n
            if (interactionText != null)
            {
                if (weaponInventory != null && player != null)
                {
                    // Obtener el ID del arma equipada
                    int currentID = weaponInventory.GetCurrentWeaponID();

                    // Verificar si el ID del arma equipada coincide con el 'weaponIndex'
                    if (currentID == weaponIndex)
                    {
                        // Si el jugador tiene el arma, mostrar el mensaje de compra de munici�n
                        int refillCost = pointCost / 2;
                        if (player.currentPoints >= refillCost)
                        {
                            interactionText.text = "Comprar munici�n: " + refillCost + " puntos";
                        }
                        else
                        {
                            interactionText.text = "No tienes suficientes puntos para munici�n.";
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
        // Verificar si el objeto que sali� del trigger tiene el tag "Player"
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            // Solo cambiar el mensaje si ya no hay interacci�n posible
            if (interactionText != null)
            {
                interactionText.text = defaultMessage;  // Restaurar el mensaje por defecto
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
            if (player == null)
            {
                Debug.LogError("No se encontr� el script Player.");
                return;  // Si no se encuentra el script del jugador, salimos de la funci�n
            }

            // Obtener el script "WeaponInventory" del objeto con tag "Player"
            if (weaponInventory == null)
            {
                Debug.Log("No se encontr� el inventory");
                return;  // Si no se encuentra el inventario, salimos de la funci�n
            }

            // Obtener el ID del arma equipada
            int currentID = weaponInventory.GetCurrentWeaponID();

            // Verificar si el ID del arma equipada coincide con el 'weaponIndex'
            if (currentID == weaponIndex)
            {
                // El jugador ya tiene el arma, ofrecer recargar munici�n a la mitad del precio original
                int refillCost = pointCost / 2;

                if (player.currentPoints >= refillCost)
                {
                    // Descontar los puntos para la recarga de munici�n
                    player.RemovePoints(refillCost);

                    // Llamar a la funci�n de recarga de munici�n
                    PlayerShooting playerShooting = GameObject.Find("Weapons").GetComponent<PlayerShooting>();
                    if (playerShooting != null)
                    {
                        playerShooting.RefillAmmo();
                    }
                    else
                    {
                        Debug.LogWarning("PlayerShooting no se encontr�");
                        return;
                    }

                    // Cambiar el mensaje de interacci�n despu�s de comprar
                    Debug.Log("Munici�n recargada por la mitad del precio.");
                }
                else
                {
                    // Si no tiene suficientes puntos para recargar munici�n
                    Debug.Log("No tienes suficientes puntos para recargar munici�n.");
                }
            }
            else
            {
                // El jugador no tiene el arma, permitir la compra del arma
                if (player.currentPoints >= pointCost)
                {
                    // Descontar los puntos por el costo
                    player.RemovePoints(pointCost);

                    // Llamar a la funci�n "OverwriteCurrentWeapon" en el script "WeaponInventory" con el �ndice configurado
                    weaponInventory.OverwriteCurrentWeapon(weaponIndex);
                    PlayerShooting playerShooting = GameObject.Find("Weapons").GetComponent<PlayerShooting>();
                    playerShooting.ApplyRotationShake();

                    // Cambiar el mensaje de interacci�n despu�s de comprar
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
