using UnityEngine;
using UnityEngine.InputSystem;  // Necesario para el Input System

public class PlayerMovementAudio : MonoBehaviour
{
    [Header("=== Sound Settings ===")]
    [Tooltip("El sonido que se reproduce mientras el jugador camina.")]
    public AudioClip walkSound;  // Clip de sonido para caminar

    [Tooltip("El sonido que se reproduce mientras el jugador corre.")]
    public AudioClip sprintSound;  // Clip de sonido para correr

    private AudioSource walkAudioSource;  // AudioSource para el sonido de caminar
    private AudioSource sprintAudioSource;  // AudioSource para el sonido de correr

    private InputAction moveAction;  // Acción de movimiento
    private InputAction sprintAction;  // Acción de sprint

    private bool isSprinting = false;  // Indicador de si está corriendo

    private void Start()
    {
        // Crear y configurar los AudioSources para caminar y correr
        walkAudioSource = gameObject.AddComponent<AudioSource>();
        walkAudioSource.clip = walkSound;
        walkAudioSource.loop = true;  // Sonido en bucle
        walkAudioSource.playOnAwake = false;  // No reproducir automáticamente al inicio

        sprintAudioSource = gameObject.AddComponent<AudioSource>();
        sprintAudioSource.clip = sprintSound;
        sprintAudioSource.loop = true;  // Sonido en bucle
        sprintAudioSource.playOnAwake = false;  // No reproducir automáticamente al inicio

        // Obtener las acciones del Input System
        var playerInputActions = new InputSystem_Actions(); // Asegúrate de que esta clase es la generada por el Input System
        moveAction = playerInputActions.Player.Move;  // "Move" es la acción que corresponde al movimiento
        sprintAction = playerInputActions.Player.Sprint;  // "Sprint" es la acción que corresponde al sprint

        // Suscribir a los eventos de las acciones
        moveAction.performed += OnMovePerformed;
        moveAction.canceled += OnMoveCanceled;

        sprintAction.performed += OnSprintPerformed;
        sprintAction.canceled += OnSprintCanceled;

        // Habilitar las acciones
        moveAction.Enable();
        sprintAction.Enable();
    }

    private void OnDisable()
    {
        // Deshabilitar las acciones y cancelar la suscripción
        moveAction.Disable();
        sprintAction.Disable();

        moveAction.performed -= OnMovePerformed;
        moveAction.canceled -= OnMoveCanceled;

        sprintAction.performed -= OnSprintPerformed;
        sprintAction.canceled -= OnSprintCanceled;
    }

    private void OnMovePerformed(InputAction.CallbackContext context)
    {
        // Si está moviéndose y no está corriendo, reproduce el sonido de caminar
        if (!isSprinting && !walkAudioSource.isPlaying)
        {
            walkAudioSource.Play();
        }
    }

    private void OnMoveCanceled(InputAction.CallbackContext context)
    {
        // Si deja de moverse, detener el sonido de caminar
        if (walkAudioSource.isPlaying)
        {
            walkAudioSource.Stop();
        }
    }

    private void OnSprintPerformed(InputAction.CallbackContext context)
    {
        // Si está corriendo, detener el sonido de caminar y reproducir el sonido de sprint
        if (!sprintAudioSource.isPlaying)
        {
            // Detener el sonido de caminar si está sonando
            if (walkAudioSource.isPlaying)
            {
                walkAudioSource.Stop();
            }

            // Reproducir el sonido de sprint
            sprintAudioSource.Play();
            isSprinting = true;
        }
    }

    private void OnSprintCanceled(InputAction.CallbackContext context)
    {
        // Si deja de correr, detener el sonido de sprint y reproducir el sonido de caminar si está moviéndose
        if (sprintAudioSource.isPlaying)
        {
            sprintAudioSource.Stop();
            isSprinting = false;
        }

        // Reproducir el sonido de caminar si está moviéndose
        if (!walkAudioSource.isPlaying && moveAction.ReadValue<Vector2>().magnitude > 0)
        {
            walkAudioSource.Play();
        }
    }
}
