using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
public class Zombie : MonoBehaviour
{
    [Header("=== ZOMBIE SETTINGS ===")]
    public float maxHealth = 100f;
    public float health = 100f; // Salud del zombi
    private NavMeshAgent agent; // Agente de navegaci�n para movimiento
    private ZombieSpawner spawner; // Referencia al ZombieSpawner
    private Animator animator; // Referencia al Animator
    private float originalSpeed; // Velocidad original del agente

    private Player playerScript; // Referencia al script Player

    [Header("=== DAMAGE SETTINGS ===")]
    [Tooltip("Damage that the zombie deals when it touches the player.")]
    public float damageAmount = 10f;

    [Tooltip("Detection range to trigger damage.")]
    public float damageRange = 2f; // Distancia a la que el zombi inflige da�o

    [Tooltip("Time in seconds before the zombie attacks once in range.")]
    public float attackDelay = 2f; // Retraso antes de atacar al jugador

    private Transform player; // Referencia al jugador
    private float attackTimer = 0f; // Temporizador para ataques
    private bool playerInRange = false; // Indica si el jugador est� en rango

    [Header("=== NAVIGATION SETTINGS ===")]
    [Tooltip("Target that the NavMeshAgent will move towards.")]
    private Transform target;  // Now private to find automatically

    [Header("=== ANIMATION SETTINGS ===")]
    [Tooltip("Animator variable to update based on agent speed.")]
    public string animatorSpeedVariable = "speed";

    private void Awake()
    {
        // Obtener referencias a los componentes necesarios
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();

        // Buscar el ZombieSpawner
        spawner = Object.FindFirstObjectByType<ZombieSpawner>();
    }

    private void Start()
    {
        health = maxHealth;
        originalSpeed = agent.speed; // Guardar la velocidad original

        // Buscar el objeto con la etiqueta "Player" y obtener su Transform
        player = GameObject.FindGameObjectWithTag("Player")?.transform;

        if (player != null)
        {
            target = player; // Asignar el jugador como objetivo
            playerScript = player.GetComponent<Player>();
        }
        else
        {
            Debug.LogError("Player with 'Player' tag not found in the scene.");
        }
    }

    private void Update()
    {
        // Verificar que el jugador est� presente antes de mover al zombi
        if (target != null && !IsDead())
        {
            MoveTowardsTarget();
        }

        // Manejar el da�o al jugador
        if (!IsDead())
        {
            HandlePlayerDamage();
        }

        // Actualizar la animaci�n de velocidad
        UpdateAnimator();
    }

    // Funci�n para hacerle da�o al zombi
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    // Funci�n que maneja la muerte del zombi
    private void Die()
    {
        // Detener el movimiento del agente
        agent.speed = 0;

        // Activar la animaci�n de muerte
        animator.SetBool("die", true);

        // Agregar puntos al jugador al morir el zombi
        if (playerScript != null)
        {
            playerScript.AddPoints(60); // Agregar 60 puntos al jugador
        }

        // Esperar 3 segundos antes de desactivar el zombi
        StartCoroutine(WaitAndDeactivate(3f)); // El tiempo de espera es de 3 segundos
    }

    private IEnumerator WaitAndDeactivate(float waitTime)
    {
        // Esperar el tiempo de la animaci�n de muerte
        yield return new WaitForSeconds(waitTime);

        // Desactivar el zombi despu�s de la animaci�n
        gameObject.SetActive(false);

        // Si el spawner se encuentra, se llama a ZombieKilled
        if (spawner != null)
        {
            spawner.ZombieKilled(); // Llamamos a la funci�n ZombieKilled en ZombieSpawner
        }

        // Restaurar la velocidad del agente a la velocidad original
        agent.speed = originalSpeed;

        // Volver a desactivar el par�metro "die" en el Animator
        animator.SetBool("die", false);

        // Restaurar la vida a su vida m�xima
        resetHealth();
    }

    private void resetHealth()
    {
        health = maxHealth;
    }

    // Funci�n para manejar el da�o al jugador
    private void HandlePlayerDamage()
    {
        if (player != null)
        {
            // Calcular distancia entre el zombi y el jugador
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            // Verificar si el jugador est� en rango de da�o
            if (distanceToPlayer <= damageRange)
            {
                // Si el jugador est� en rango, iniciar el temporizador de ataque
                if (!playerInRange)
                {
                    playerInRange = true;
                    attackTimer = attackDelay; // Reiniciar temporizador de ataque
                }

                // Contar hacia atr�s el temporizador de ataque
                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }

                // Si el temporizador llega a 0, atacar al jugador
                if (attackTimer <= 0)
                {
                    player.GetComponent<Player>().TakeDamage(damageAmount);
                    attackTimer = attackDelay; // Reiniciar temporizador de ataque
                }
            }
            else
            {
                // Si el jugador est� fuera de rango, reiniciar el temporizador de ataque
                if (playerInRange)
                {
                    playerInRange = false;
                    attackTimer = attackDelay; // Reiniciar temporizador
                }
            }
        }
    }

    /// <summary>
    /// Mueve al zombi hacia su objetivo (jugador)
    /// </summary>
    private void MoveTowardsTarget()
    {
        agent.SetDestination(target.position); // Mover hacia el objetivo (jugador)
    }

    /// <summary>
    /// Actualiza el par�metro de velocidad en el Animator basado en la velocidad del NavMeshAgent.
    /// </summary>
    private void UpdateAnimator()
    {
        if (animator != null && !string.IsNullOrEmpty(animatorSpeedVariable))
        {
            // Calcular la velocidad actual del NavMeshAgent
            float speed = agent.velocity.magnitude;

            // Actualizar el par�metro en el Animator
            animator.SetFloat(animatorSpeedVariable, speed);
        }
    }

    /// <summary>
    /// Establece un nuevo objetivo para el NavMeshAgent.
    /// </summary>
    /// <param name="newTarget">El nuevo objetivo (Transform) al que mover el agente.</param>
    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    /// <summary>
    /// Obtiene el objetivo actual del NavMeshAgent.
    /// </summary>
    /// <returns>El Transform del objetivo actual.</returns>
    public Transform GetCurrentTarget()
    {
        return target;
    }

    // Funci�n para verificar si el zombi est� muerto
    private bool IsDead()
    {
        // Verificamos si la animaci�n de muerte est� activa o si la salud es 0
        return animator.GetBool("die") || health <= 0;
    }
}
