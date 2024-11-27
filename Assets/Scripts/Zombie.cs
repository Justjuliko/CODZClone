using System.Collections;
using UnityEngine;
using UnityEngine.AI;

public class Zombie : MonoBehaviour
{
    [Header("=== ZOMBIE SETTINGS ===")]
    public float maxHealth = 100f;
    public float health = 100f;  // Salud del zombi
    private NavMeshAgent agent;  // Agente de navegaci�n para movimiento
    private ZombieSpawner spawner;  // Referencia al ZombieSpawner
    private Animator animator;  // Referencia al Animator
    private float originalSpeed;  // Velocidad original del agente

    private Player playerScript;  // Referencia al script Player

    private void Awake()
    {
        // Buscar el ZombieSpawner usando el m�todo actualizado
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

        // Buscar el objeto con la etiqueta "Player" y obtener su script Player
        playerScript = GameObject.FindGameObjectWithTag("Player").GetComponent<Player>();
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
            playerScript.AddPoints(60);  // Agregar 60 puntos al jugador
        }

        // Esperar 3 segundos antes de desactivar el zombi
        StartCoroutine(WaitAndDeactivate(3f));  // El tiempo de espera es de 3 segundos
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
            spawner.ZombieKilled();  // Llamamos a la funci�n ZombieKilled en ZombieSpawner
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
}
