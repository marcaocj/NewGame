using UnityEngine;

public class EnemyStats : MonoBehaviour
{
    public enum EnemyBehaviorType { Passive, Aggressive }
    public EnemyBehaviorType behaviorType = EnemyBehaviorType.Aggressive;

    public float maxHealth = 100f;
    public float currentHealth;

    public float attackDamage = 10f;
    public float attackRange = 2f;
    public float attackCooldown = 2f;
    public int experienceReward = 50;

    [Header("Detecção")]
    public float detectionRadius = 8f;
    public LayerMask playerLayer;

    private float lastAttackTime;

    private Transform player;
    private PlayerStats playerStats;
    private EnemyWander wanderScript;

    private void Start()
    {
        currentHealth = maxHealth;
        player = GameObject.FindGameObjectWithTag("Player")?.transform;
        playerStats = player?.GetComponent<PlayerStats>();
        wanderScript = GetComponent<EnemyWander>();
    }

    private void Update()
    {
        if (behaviorType == EnemyBehaviorType.Passive || player == null || playerStats == null || playerStats.IsDead())
            return;

        float distance = Vector3.Distance(transform.position, player.position);
        bool playerInSight = distance <= detectionRadius;

        if (playerInSight)
        {
            // Desativa patrulha e persegue o player
            if (wanderScript && wanderScript.enabled)
                wanderScript.enabled = false;

            // Perseguir usando Rigidbody (sem NavMesh)
            Vector3 direction = (player.position - transform.position).normalized;
            direction.y = 0f;

            GetComponent<Rigidbody>().linearVelocity = direction * 3f;

            transform.rotation = Quaternion.Slerp(transform.rotation,
                                                  Quaternion.LookRotation(direction),
                                                  Time.deltaTime * 5f);

            if (distance <= attackRange && Time.time >= lastAttackTime + attackCooldown)
            {
                AttackPlayer();
            }
        }
        else
        {
            // Volta a patrulhar
            if (wanderScript && !wanderScript.enabled)
            {
                wanderScript.enabled = true;
                GetComponent<Rigidbody>().linearVelocity = Vector3.zero;
            }
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth -= amount;

        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    private void AttackPlayer()
    {
        lastAttackTime = Time.time;
        playerStats.TakeDamage(attackDamage);
        Debug.Log("Inimigo atacou o player!");
    }

    private void Die()
    {
        Debug.Log("Inimigo morreu!");
        if (player != null)
        {
            player.GetComponent<ExperienceSystem>()?.GainXP(experienceReward);
        }

        Destroy(gameObject);
    }
}
