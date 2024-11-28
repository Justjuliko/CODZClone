using System.Collections;
using UnityEngine;
using UnityEngine.Events;
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

    Player player;

    [SerializeField] UnityEvent spawnEvent;

    [Header("=== Purchase Sound Settings ===")]
    [SerializeField] private AudioClip purchaseSound; // Clip de sonido para la compra
    private AudioSource purchaseAudioSource; // AudioSource dedicado al sonido de compra

    private void Start()
    {
        player = GameObject.Find("FirstPersonController").GetComponent<Player>();

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

                // Reproducir el sonido de compra
                PlayPurchaseSound();

                // Evento que enciende los spawns
                spawnEvent.Invoke();

                // Desactivar BoxCollider mientras suena el sonido
                DisableCollidersTemporarily();

                // Limpiar el mensaje de interacción antes de desactivar el objeto
                if (interactionText != null)
                {
                    interactionText.text = "";  // Limpiar el mensaje
                }

                // Desactivar el objeto después de que el sonido termine
                StartCoroutine(DeactivateAfterSound());
            }
            else
            {
                // Si no tiene suficientes puntos, mostrar un mensaje
                Debug.Log("No tienes suficientes puntos para realizar la acción.");
            }
        }
    }

    private void PlayPurchaseSound()
    {
        if (purchaseSound != null && purchaseAudioSource != null)
        {
            purchaseAudioSource.volume = 1f;  // Asegúrate de que el volumen esté al máximo
            purchaseAudioSource.spatialBlend = 0f;  // Esto asegurará que el sonido sea 2D, no 3D
            purchaseAudioSource.Play();
        }
    }

    private void DisableCollidersTemporarily()
    {
        // Desactivar ambos BoxColliders temporalmente
        Collider[] colliders = GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = false;
        }
    }

    private IEnumerator DeactivateAfterSound()
    {
        // Espera a que el sonido termine
        yield return new WaitForSeconds(purchaseAudioSource.clip.length);

        // Reactivar los BoxColliders después del sonido
        Collider[] colliders = GetComponents<Collider>();
        foreach (var collider in colliders)
        {
            collider.enabled = true;
        }

        // Desactivar el objeto
        gameObject.SetActive(false);
    }
}
