using UnityEngine;
using UnityEngine.UI;

public class EnemyStats2 : MonoBehaviour
{
    [Header("Stats Settings")]
    public int maxHealth = 100;
    public int currentHealth;
    public int attackPower = 10;
    public float attackRange = 1.5f;
    public float attackCooldown = 2f;
    
    [Header("UI Elements (Optional)")]
    public Slider healthBar;
    public GameObject damageTextPrefab;
    
    [Header("Death Settings")]
    public bool destroyOnDeath = true;
    public float destroyDelay = 2f;
    public GameObject deathEffectPrefab;
    
    // Status effects
    private bool isStunned = false;
    private float stunTimer = 0f;
    private float lastAttackTime = 0f;
    
    private Animator animator;
    private EnemyWander wanderComponent;
    
    private void Awake()
    {
        currentHealth = maxHealth;
        animator = GetComponentInChildren<Animator>();
        wanderComponent = GetComponent<EnemyWander>();
        
        // Configure a barra de vida se existir
        if (healthBar != null)
        {
            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
        }
    }
    
    private void Update()
    {
        // Atualiza o timer de atordoamento se estiver atordoado
        if (isStunned)
        {
            stunTimer -= Time.deltaTime;
            if (stunTimer <= 0)
            {
                isStunned = false;
                // Retoma o comportamento normal
                if (wanderComponent != null)
                {
                    wanderComponent.enabled = true;
                }
            }
        }
    }
    
    // Método para receber dano
    public void TakeDamage(int damage, bool canStun = false, float stunDuration = 1f)
    {
        if (currentHealth <= 0)
            return; // Já está morto
            
        currentHealth -= damage;
        
        // Efeito visual de dano (opcional)
        ShowDamageEffect(damage);
        
        // Atualiza a barra de vida, se existir
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
        
        // Verifica se o inimigo foi atordoado
        if (canStun && !isStunned)
        {
            isStunned = true;
            stunTimer = stunDuration;
            
            // Pausa o movimento durante o atordoamento
            if (wanderComponent != null)
            {
                wanderComponent.enabled = false;
            }
        }
        
        // Ativa animação de dano, se existir
        if (animator != null)
        {
            animator.SetTrigger("Hit");
        }
        
        // Verifica se o inimigo morreu
        if (currentHealth <= 0)
        {
            Die();
        }
    }
    
    // Método para curar o inimigo
    public void Heal(int amount)
    {
        currentHealth = Mathf.Min(currentHealth + amount, maxHealth);
        
        // Atualiza a barra de vida, se existir
        if (healthBar != null)
        {
            healthBar.value = currentHealth;
        }
    }
    
    // Método para atacar o jogador ou outro alvo
    public bool TryAttack(Transform target)
    {
        // Verifica se o cooldown passou
        if (Time.time - lastAttackTime < attackCooldown)
            return false;
            
        // Verifica se o alvo está no alcance
        float distance = Vector3.Distance(transform.position, target.position);
        if (distance > attackRange)
            return false;
            
        // Executa o ataque
        lastAttackTime = Time.time;
        
        // Ativa a animação de ataque, se existir
        if (animator != null)
        {
            animator.SetTrigger("Attack");
        }
        
        // A lógica real de dano seria implementada no sistema de combate
        // Por enquanto, apenas retorna sucesso
        return true;
    }
    
    // Método para morte do inimigo
    private void Die()
    {
        // Ativa a animação de morte, se existir
        if (animator != null)
        {
            animator.SetTrigger("Death");
        }
        
        // Desativa os componentes de movimento e colisão
        if (wanderComponent != null)
        {
            wanderComponent.enabled = false;
        }
        
        Collider col = GetComponent<Collider>();
        if (col != null)
        {
            col.enabled = false;
        }
        
        Rigidbody rb = GetComponent<Rigidbody>();
        if (rb != null)
        {
            rb.isKinematic = true;
        }
        
        // Spawna o efeito de morte, se configurado
        if (deathEffectPrefab != null)
        {
            Instantiate(deathEffectPrefab, transform.position, Quaternion.identity);
        }
        
        // Destroi o objeto após um delay
        if (destroyOnDeath)
        {
            Destroy(gameObject, destroyDelay);
        }
    }
    
    // Método para mostrar efeito de dano (opcional)
    private void ShowDamageEffect(int damage)
    {
        // Se tiver um prefab para texto de dano, instancia ele
        if (damageTextPrefab != null)
        {
            GameObject damageObj = Instantiate(damageTextPrefab, transform.position + Vector3.up * 2f, Quaternion.identity);
            Text damageText = damageObj.GetComponent<Text>();
            if (damageText != null)
            {
                damageText.text = damage.ToString();
            }
            
            // Destrói o texto após alguns segundos
            Destroy(damageObj, 1f);
        }
    }
}