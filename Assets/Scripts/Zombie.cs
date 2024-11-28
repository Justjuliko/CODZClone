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
    private NavMeshAgent agent;
    private ZombieSpawner spawner;
    private Animator animator;
    private CapsuleCollider capsuleCollider;
    public float originalSpeed = 1;

    private Player playerScript;

    [Header("=== DAMAGE SETTINGS ===")]
    public float damageAmount = 10f;
    public float damageRange = 2f;
    public float attackDelay = 2f;

    private Transform player;
    private float attackTimer = 0f;
    private bool playerInRange = false;

    [Header("=== NAVIGATION SETTINGS ===")]
    private Transform target;

    [Header("=== ANIMATION SETTINGS ===")]
    public string animatorSpeedVariable = "speed";
    public string animatorAttackVariable = "attack";

    [Header("=== AUDIO SETTINGS ===")]
    [Tooltip("Array of looping sounds to play randomly.")]
    public AudioClip[] loopSounds; // Array para almacenar múltiples sonidos de bucle
    private AudioSource loopAudioSource;
    public float minLoopInterval = 5f;
    public float maxLoopInterval = 10f;

    [Tooltip("Sound to play when the zombie attacks.")]
    public AudioClip attackSound;
    private AudioSource attackAudioSource;

    [Tooltip("Sound to play when the zombie spawns.")]
    public AudioClip spawnSound; // Sonido al spawnear
    private AudioSource spawnAudioSource; // AudioSource dedicado al spawn
    public float spawnSoundVolume = 1.0f; // Volumen del sonido de spawn


    private void Awake()
    {
        // Componentes necesarios
        agent = GetComponent<NavMeshAgent>();
        animator = GetComponent<Animator>();
        capsuleCollider = GetComponent<CapsuleCollider>();
        spawner = Object.FindFirstObjectByType<ZombieSpawner>();

        // Configurar AudioSources
        loopAudioSource = gameObject.AddComponent<AudioSource>();
        attackAudioSource = gameObject.AddComponent<AudioSource>();
        spawnAudioSource = gameObject.AddComponent <AudioSource>();

        // Configuración del AudioSource de bucle
        loopAudioSource.loop = false;
        loopAudioSource.playOnAwake = false;

        // Configuración del AudioSource de ataque
        attackAudioSource.clip = attackSound;
        attackAudioSource.loop = false;
        attackAudioSource.playOnAwake = false;

        // Configuración del AudioSource de spawn
        spawnAudioSource.clip = spawnSound;
        spawnAudioSource.loop = false;
        spawnAudioSource.playOnAwake = false;
        spawnAudioSource.volume = spawnSoundVolume; // Asignar volumen personalizado

        // Habilitar sonido espacial
        ConfigureSpatialAudio(loopAudioSource);
        ConfigureSpatialAudio(attackAudioSource);
        ConfigureSpatialAudio(spawnAudioSource);
    }

    private void ConfigureSpatialAudio(AudioSource audioSource)
    {
        audioSource.spatialBlend = 1f; // 100% espacial
        audioSource.minDistance = 2f; // Volumen máximo a esta distancia
        audioSource.maxDistance = 15f; // Volumen mínimo más allá de esta distancia
        audioSource.rolloffMode = AudioRolloffMode.Linear; // Atenuación lineal
    }

    private void Start()
    {
        health = maxHealth;

        // Buscar al jugador
        player = GameObject.Find("FirstPersonController")?.transform;

        if (player != null)
        {
            target = player;
            playerScript = player.GetComponent<Player>();
        }
        else
        {
            Debug.LogError("Player not found in the scene.");
        }

        // Iniciar reproducción de sonido en intervalos aleatorios
        StartCoroutine(PlayRandomLoopSound());
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
        animator.SetBool(animatorAttackVariable, false);
        capsuleCollider.enabled = false;

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
        capsuleCollider.enabled = true;
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
                    animator.SetBool(animatorAttackVariable, true);
                }

                if (attackTimer > 0)
                {
                    attackTimer -= Time.deltaTime;
                }

                if (attackTimer <= 0)
                {
                    playerScript.TakeDamage(damageAmount);
                    attackAudioSource.Play(); // Reproducir sonido de ataque
                    attackTimer = attackDelay;
                }
            }
            else
            {
                if (playerInRange)
                {
                    playerInRange = false;
                    attackTimer = attackDelay;
                    animator.SetBool(animatorAttackVariable, false);
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

    private IEnumerator PlayRandomLoopSound()
    {
        while (true)
        {
            float randomDelay = Random.Range(minLoopInterval, maxLoopInterval);
            yield return new WaitForSeconds(randomDelay);

            if (loopSounds.Length > 0)
            {
                // Seleccionar un sonido aleatorio
                int randomIndex = Random.Range(0, loopSounds.Length);
                loopAudioSource.clip = loopSounds[randomIndex];
                loopAudioSource.Play();

                // Esperar hasta que el sonido termine
                yield return new WaitForSeconds(loopAudioSource.clip.length);
            }
        }
    }

    public IEnumerator HandleSpawn()
    {
        agent.speed = 0;
        animator.SetBool("spawn", true);

        // Reproducir el sonido de spawn si está configurado
        if (spawnSound != null)
        {
            spawnAudioSource.gameObject.SetActive(true);
            spawnAudioSource.enabled = true;
            spawnAudioSource.Play();
        }

        yield return new WaitForSeconds(1.4f);
        animator.SetBool("spawn", false);

        if (spawnAudioSource.isPlaying)
        {
            spawnAudioSource.Stop();
        }

        agent.speed = originalSpeed;
    }
}
