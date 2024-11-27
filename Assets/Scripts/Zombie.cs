using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [Header("=== ZOMBIE SETTINGS ===")]
    public float maxHealth = 100f;
    public float health = 100f;  // Salud del zombi
    private NavMeshAgent agent;  // Agente de navegación para movimiento
    private ZombieSpawner spawner;  // Referencia al ZombieSpawner
    private Animator animator;  // Referencia al Animator
    private float originalSpeed;  // Velocidad original del agente

    private void Awake()
    {
        // Buscar el ZombieSpawner usando el método actualizado
        spawner = Object.FindFirstObjectByType<ZombieSpawner>();

        // Obtener el componente Animator
        animator = GetComponent<Animator>();

        // Obtener el componente NavMeshAgent
        agent = GetComponent<NavMeshAgent>();
    }

    private void Start()
    {
        health = maxHealth;
        originalSpeed = agent.speed;  // Guardar la velocidad original
    }

    // Función para hacerle daño al zombi
    public void TakeDamage(float damage)
    {
        health -= damage;

        if (health <= 0)
        {
            Die();
        }
    }

    // Función que maneja la muerte del zombi
    private void Die()
    {
        // Detener el movimiento del agente
        agent.speed = 0;

        // Activar la animación de muerte
        animator.SetBool("die", true);

        // Esperar 3 segundos antes de desactivar el zombi
        StartCoroutine(WaitAndDeactivate(3f));  // El tiempo de espera es de 3 segundos
    }

    private IEnumerator WaitAndDeactivate(float waitTime)
    {
        // Esperar el tiempo de la animación de muerte
        yield return new WaitForSeconds(waitTime);

        // Desactivar el zombi después de la animación
        gameObject.SetActive(false);

        // Si el spawner se encuentra, se llama a ZombieKilled
        if (spawner != null)
        {
            spawner.ZombieKilled();  // Llamamos a la función ZombieKilled en ZombieSpawner
        }

        // Restaurar la velocidad del agente a la velocidad original
        agent.speed = originalSpeed;

        // Volver a desactivar el parámetro "die" en el Animator
        animator.SetBool("die", false);

        // Restaurar la vida a su vida máxima
        resetHealth();
    }

    private void resetHealth()
    {
        health = maxHealth;
    }
}
