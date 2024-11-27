using System.Collections;
using UnityEngine;
using UnityEngine.AI;

[RequireComponent(typeof(NavMeshAgent))]
[RequireComponent(typeof(Animator))]
[RequireComponent(typeof(CapsuleCollider))]
public class Zombie : MonoBehaviour
{
    [Header("=== ZOMBIE SETTINGS ===")]
    public float maxHealth = 100f;
    public float health = 100f; // Salud del zombi
    private NavMeshAgent agent; // Agente de navegación para movimiento
    private ZombieSpawner spawner; // Referencia al ZombieSpawner
    private Animator animator; // Referencia al Animator
    private CapsuleCollider capsuleCollider; // Referencia al CapsuleCollider
    private float originalSpeed; // Velocidad original del agente

    private Player playerScript; // Referencia al script Player

    [Header("=== DAMAGE SETTINGS ===")]
    [Tooltip("Damage that the zombie deals when it touches the player.")]
    public float damageAmount = 10f;

    [Tooltip("Detection range to trigger damage.")]
    public float damageRange = 2f; // Distancia a la que el zombi inflige daño

    [Tooltip("Time in seconds before the zombie attacks once in range.")]
    public float attackDelay = 2f; // Retraso antes de atacar al jugador

    private Transform player; // Referencia al jugador
    private float attackTimer = 0f; // Temporizador para ataques
    private bool playerInRange = false; // Indica si el jugador está en rango

    [Header("=== NAVIGATION SETTINGS ===")]
    [Tooltip("Target that the NavMeshAgent will move towards.")]
    private Transform target;  // Now private to find automatically

    [Header("=== ANIMATION SETTINGS ===")]
    [Tooltip("Animator variable to update based on agent speed.")]
    public string animatorSpeedVariable = "speed";
    public string animatorAttackVariable = "attack"; // Parámetro de ataque en el Animator

    private void Awake()
    {
        // Obtener referencias a los componentes necesarios
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();

        // Buscar el ZombieSpawner
        spawner = Object.FindFirstObjectByType<ZombieSpawner>();
    }

    private void Start()
    {
        health = maxHealth;
        originalSpeed = agent.speed; // Guardar la velocidad original

        // Buscar el objeto con la etiqueta "Player" y obtener su Transform
        player = GameObject.Find("FirstPersonController")?.transform;

        if (player != null)
        {
            target = player; // Asignar el jugador como objetivo
            playerScript = player.GetComponent<Player>();
        }
        else
        {
            Debug.LogError("Player not found in the scene.");
        }
    }

    private void Update()
    {
        if (target != null && !IsDead())
        {
            MoveTowardsTarget();
            HandlePlayerDamage();
        }

        UpdateAnimator();
    }

    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    private void Die()
    {
        agent.speed = 0;
        animator.SetBool("die", true);
        animator.SetBool(animatorAttackVariable, false); // Asegurarse de que ataque esté en falso
        capsuleCollider.enabled = false; // Apagar el collider

        if (playerScript != null)
        {
            playerScript.AddPoints(60);
        }

        StartCoroutine(WaitAndDeactivate(3f));
    }

    private IEnumerator WaitAndDeactivate(float waitTime)
    {
        yield return new WaitForSeconds(waitTime);
        gameObject.SetActive(false);

        if (spawner != null)
        {
            spawner.ZombieKilled();
        }

        agent.speed = originalSpeed;
        animator.SetBool("die", false);
        health = maxHealth;
        capsuleCollider.enabled = true; // Volver a activar el collider
    }

    private void HandlePlayerDamage()
    {
        if (player != null)
        {
            float distanceToPlayer = Vector3.Distance(transform.position, player.position);

            if (distanceToPlayer <= damageRange)
            {
                if (!playerInRange)
                {
                    playerInRange = true;
                    attackTimer = attackDelay;
                    animator.SetBool(animatorAttackVariable, true); // Activar parámetro de ataque
                }

                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }

                if (attackTimer <= 0)
                {
                    player.GetComponent<Player>().TakeDamage(damageAmount);
                    attackTimer = attackDelay;
                }
            }
            else
            {
                if (playerInRange)
                {
                    playerInRange = false;
                    attackTimer = attackDelay;
                    animator.SetBool(animatorAttackVariable, false); // Desactivar parámetro de ataque
                }
            }
        }
    }

    private void MoveTowardsTarget()
    {
        agent.SetDestination(target.position);
    }

    private void UpdateAnimator()
    {
        if (animator != null && !string.IsNullOrEmpty(animatorSpeedVariable))
        {
            float speed = agent.velocity.magnitude;
            animator.SetFloat(animatorSpeedVariable, speed);
        }
    }

    public void SetTarget(Transform newTarget)
    {
        target = newTarget;
    }

    public Transform GetCurrentTarget()
    {
        return target;
    }

    private bool IsDead()
    {
        return animator.GetBool("die") || health <= 0;
    }
}
