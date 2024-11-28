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

    [Header("=== Purchase Sound Settings ===")]
    public AudioClip purchaseSound; // Clip de sonido para la compra
    private AudioSource purchaseAudioSource; // AudioSource dedicado al sonido de compra

    private void Start()
    {
        player = GameObject.Find("FirstPersonController").GetComponent<Player>();
        weaponInventory = GameObject.FindGameObjectWithTag("Player").GetComponent<WeaponInventory>();

        // Configurar el AudioSource para el sonido de compra
        purchaseAudioSource = gameObject.AddComponent<AudioSource>();
        purchaseAudioSource.clip = purchaseSound;
        purchaseAudioSource.playOnAwake = false;
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
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = true;

            if (interactionText != null)
            {
                if (weaponInventory != null && player != null)
                {
                    int currentID = weaponInventory.GetCurrentWeaponID();

                    if (currentID == weaponIndex)
                    {
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
        if (other.CompareTag("Player"))
        {
            isPlayerInRange = false;

            if (interactionText != null)
            {
                interactionText.text = defaultMessage;  // Restaurar el mensaje por defecto
            }
        }
    }

    private void OnInteractPerformed(InputAction.CallbackContext context)
    {
        if (isPlayerInRange)
        {
            if (player == null)
            {
                Debug.LogError("No se encontró el script Player.");
                return;
            }

            if (weaponInventory == null)
            {
                Debug.Log("No se encontró el inventory");
                return;
            }

            int currentID = weaponInventory.GetCurrentWeaponID();

            if (currentID == weaponIndex)
            {
                int refillCost = pointCost / 2;

                if (player.currentPoints >= refillCost)
                {
                    player.RemovePoints(refillCost);

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

                    PlayPurchaseSound(); // Reproducir el sonido de compra
                    Debug.Log("Munición recargada por la mitad del precio.");
                }
                else
                {
                    Debug.Log("No tienes suficientes puntos para recargar munición.");
                }
            }
            else
            {
                if (player.currentPoints >= pointCost)
                {
                    player.RemovePoints(pointCost);
                    weaponInventory.OverwriteCurrentWeapon(weaponIndex);

                    PlayerShooting playerShooting = GameObject.Find("Weapons").GetComponent<PlayerShooting>();

                    PlayPurchaseSound(); // Reproducir el sonido de compra

                    if (interactionText != null)
                    {
                        interactionText.text = "Compraste el arma por " + pointCost + " puntos";
                    }

                    Debug.Log("Arma comprada.");
                }
                else
                {
                    Debug.Log("No tienes suficientes puntos para comprar el arma.");
                }
            }
        }
    }

    private void PlayPurchaseSound()
    {
        if (purchaseSound != null && purchaseAudioSource != null)
        {
            purchaseAudioSource.Play();
        }
    }
}
