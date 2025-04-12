using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PlayerAttack : MonoBehaviour
{
    [Header("Configurações de Ataque")]
    [SerializeField] private float baseAttackRange = 2f;
    [SerializeField] private float attackCooldown = 1f;
    [SerializeField] private GameObject attackEffectPrefab;
    [SerializeField] private Transform attackPoint;
    [SerializeField] private LayerMask enemyLayers;

    [Header("Tipos de Ataques")]
    [SerializeField] private bool useBasicAttack = true;
    [SerializeField] private bool useAbilities = true;

    [Header("Configurações Críticas")]
    [SerializeField] private float baseCritChance = 0.05f;    // 5% chance base
    [SerializeField] private float baseCritMultiplier = 1.5f; // 150% dano base

    [Header("Efeitos Sonoros")]
    [SerializeField] private AudioClip attackSound;
    [SerializeField] private AudioClip critSound;
    [SerializeField] private AudioClip missSound;

    // Parâmetros de combate
    private float nextAttackTime = 0f;
    private float currentAttackRange;
    private float currentAttackSpeed;
    private float currentCritChance;
    private float currentCritMultiplier;

    // Referências a outros componentes
    private PlayerStats playerStats;
    private PlayerClassType playerClass;
    private Animator animator;
    private AudioSource audioSource;
    private SkillSystem skillSystem;

    // Cache para otimização de performance
    private readonly Collider2D[] hitResults = new Collider2D[10];
    private WaitForSeconds attackEffectDuration;

    // Modificadores de atributos para diferentes tipos de ataque
    private Dictionary<AttackType, AttackModifiers> attackModifiers = new Dictionary<AttackType, AttackModifiers>();

    // Enum para tipos de ataque
    public enum AttackType
    {
        Basic,
        Fire,
        Ice,
        Lightning,
        Poison,
        Melee
    }

    // Struct para modificadores de ataque
    [System.Serializable]
    public struct AttackModifiers
    {
        public float strengthMultiplier;
        public float dexterityMultiplier;
        public float intelligenceMultiplier;
        public float wisdomMultiplier;
        public float luckMultiplier;
    }

    private void Awake()
    {
        // Obtém referências aos componentes necessários
        playerStats = GetComponent<PlayerStats>();
        playerClass = GetComponent<PlayerClassType>();
        animator = GetComponent<Animator>();
        audioSource = GetComponent<AudioSource>();
        skillSystem = GetComponent<SkillSystem>();

        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }

        if (attackPoint == null)
        {
            attackPoint = transform;
            Debug.LogWarning("Ponto de ataque não definido. Usando a posição do jogador.");
        }

        // Configura duração do efeito de ataque
        attackEffectDuration = new WaitForSeconds(0.5f);

        // Inicializa os modificadores de ataque baseados nas classes
        InitializeAttackModifiers();
    }

    private void Start()
    {
        // Inicializa parâmetros de combate
        UpdateCombatParameters();
    }

    private void Update()
    {
        // Verifica input para ataques básicos
        if (Input.GetMouseButtonDown(0) && Time.time >= nextAttackTime && useBasicAttack)
        {
            PerformBasicAttack();
        }

        // Verifica input para habilidades (1-4)
        if (useAbilities && skillSystem != null)
        {
            if (Input.GetKeyDown(KeyCode.Alpha1))
            {
                UseAbility(0);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha2))
            {
                UseAbility(1);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha3))
            {
                UseAbility(2);
            }
            else if (Input.GetKeyDown(KeyCode.Alpha4))
            {
                UseAbility(3);
            }
        }
    }

    private void InitializeAttackModifiers()
    {
        // Configura modificadores para cada tipo de ataque com base na classe do jogador
        
        // Ataque básico
        attackModifiers[AttackType.Basic] = new AttackModifiers
        {
            strengthMultiplier = 0.7f,
            dexterityMultiplier = 0.3f,
            intelligenceMultiplier = 0.0f,
            wisdomMultiplier = 0.0f,
            luckMultiplier = 0.1f
        };

        // Ataque de fogo (mágico)
        attackModifiers[AttackType.Fire] = new AttackModifiers
        {
            strengthMultiplier = 0.1f,
            dexterityMultiplier = 0.2f,
            intelligenceMultiplier = 0.8f,
            wisdomMultiplier = 0.2f,
            luckMultiplier = 0.1f
        };

        // Ataque de gelo (mágico)
        attackModifiers[AttackType.Ice] = new AttackModifiers
        {
            strengthMultiplier = 0.0f,
            dexterityMultiplier = 0.1f,
            intelligenceMultiplier = 0.7f,
            wisdomMultiplier = 0.3f,
            luckMultiplier = 0.1f
        };

        // Ataque de raio (mágico)
        attackModifiers[AttackType.Lightning] = new AttackModifiers
        {
            strengthMultiplier = 0.1f,
            dexterityMultiplier = 0.3f,
            intelligenceMultiplier = 0.8f,
            wisdomMultiplier = 0.1f,
            luckMultiplier = 0.2f
        };

        // Ataque de veneno (híbrido)
        attackModifiers[AttackType.Poison] = new AttackModifiers
        {
            strengthMultiplier = 0.2f,
            dexterityMultiplier = 0.4f,
            intelligenceMultiplier = 0.4f,
            wisdomMultiplier = 0.2f,
            luckMultiplier = 0.2f
        };

        // Ataque corpo a corpo (físico)
        attackModifiers[AttackType.Melee] = new AttackModifiers
        {
            strengthMultiplier = 0.8f,
            dexterityMultiplier = 0.4f,
            intelligenceMultiplier = 0.0f,
            wisdomMultiplier = 0.0f,
            luckMultiplier = 0.1f
        };

        // Ajusta modificadores com base na classe do jogador
        AdjustAttackModifiersForClass();
    }

    private void AdjustAttackModifiersForClass()
    {
        if (playerClass == null) return;

        // Ajusta modificadores com base na classe do jogador
        switch (playerClass.PlayerClass)
        {
            case ClassType.Warrior:
                // Guerreiros são melhores com ataques físicos
                BoostAttackType(AttackType.Basic, 0.2f);
                BoostAttackType(AttackType.Melee, 0.3f);
                break;

            case ClassType.Mage:
                // Magos são melhores com ataques mágicos
                BoostAttackType(AttackType.Fire, 0.3f);
                BoostAttackType(AttackType.Ice, 0.3f);
                BoostAttackType(AttackType.Lightning, 0.3f);
                break;

            case ClassType.Archer:
                // Arqueiros são melhores com destreza e venenos
                BoostAttackType(AttackType.Basic, 0.1f, "dexterity");
                BoostAttackType(AttackType.Poison, 0.2f);
                break;

            case ClassType.Cleric:
                // Clérigos são balanceados
                BoostAttackType(AttackType.Lightning, 0.2f);
                BoostAttackType(AttackType.Melee, 0.1f);
                break;
        }
    }

    private void BoostAttackType(AttackType attackType, float boost, string specificAttribute = null)
    {
        if (!attackModifiers.ContainsKey(attackType)) return;

        AttackModifiers modifiers = attackModifiers[attackType];
        
        if (specificAttribute != null)
        {
            // Boost apenas um atributo específico
            switch (specificAttribute.ToLower())
            {
                case "strength":
                    modifiers.strengthMultiplier += boost;
                    break;
                case "dexterity":
                    modifiers.dexterityMultiplier += boost;
                    break;
                case "intelligence":
                    modifiers.intelligenceMultiplier += boost;
                    break;
                case "wisdom":
                    modifiers.wisdomMultiplier += boost;
                    break;
                case "luck":
                    modifiers.luckMultiplier += boost;
                    break;
            }
        }
        else
        {
            // Boost geral a todos os atributos
            modifiers.strengthMultiplier += boost;
            modifiers.dexterityMultiplier += boost;
            modifiers.intelligenceMultiplier += boost;
            modifiers.wisdomMultiplier += boost;
            modifiers.luckMultiplier += boost;
        }

        attackModifiers[attackType] = modifiers;
    }

    private void UpdateCombatParameters()
    {
        if (playerStats == null) return;

        // Atualiza parâmetros de combate com base nos atributos do jogador
        currentAttackSpeed = playerStats.AttackSpeed;
        currentAttackRange = baseAttackRange + (playerStats.Level * 0.05f);
        
        // Atualiza chance de crítico e multiplicador
        float luckFactor = GetAttributeValue("luck") * 0.001f;  // 1 ponto de sorte = +0.1% chance crítica
        currentCritChance = baseCritChance + luckFactor;
        currentCritMultiplier = baseCritMultiplier + (luckFactor * 0.5f);
    }

    private void PerformBasicAttack()
    {
        // Seta o tempo do próximo ataque
        nextAttackTime = Time.time + (attackCooldown / currentAttackSpeed);
        
        // Executa animação
        animator?.SetTrigger("Attack");
        
        // Toca som de ataque
        PlaySound(attackSound);
        
        // Detecta inimigos no alcance
        int hits = Physics2D.OverlapCircleNonAlloc(attackPoint.position, currentAttackRange, hitResults, enemyLayers);
        
        if (hits > 0)
        {
            // Processa cada inimigo atingido
            for (int i = 0; i < hits; i++)
            {
                EnemyStats enemy = hitResults[i].GetComponent<EnemyStats>();
                if (enemy != null)
                {
                    // Calcula e aplica dano baseado nos atributos
                    float damage = CalculateDamage(AttackType.Basic);
                    ApplyDamage(enemy, damage);
                }
            }
            
            // Instancia efeito visual
            StartCoroutine(ShowAttackEffect());
        }
        else
        {
            // Errou o ataque
            PlaySound(missSound);
        }
    }

    private float CalculateDamage(AttackType attackType)
    {
        if (!attackModifiers.ContainsKey(attackType))
        {
            // Fallback para ataque básico se o tipo não estiver definido
            attackType = AttackType.Basic;
        }

        AttackModifiers modifiers = attackModifiers[attackType];
        
        // Calcula dano base usando os modificadores de atributo
        float baseDamage = 
            GetAttributeValue("strength") * modifiers.strengthMultiplier +
            GetAttributeValue("dexterity") * modifiers.dexterityMultiplier +
            GetAttributeValue("intelligence") * modifiers.intelligenceMultiplier +
            GetAttributeValue("wisdom") * modifiers.wisdomMultiplier;
            
        // Adiciona dano base do jogador
        baseDamage += playerStats.Attack;
        
        // Verifica crítico
        bool isCritical = Random.value <= (currentCritChance + GetAttributeValue("luck") * modifiers.luckMultiplier * 0.001f);
        
        if (isCritical)
        {
            // Aplica multiplicador crítico
            baseDamage *= currentCritMultiplier;
            PlaySound(critSound);
            
            // Efeito visual especial para críticos pode ser adicionado aqui
        }
        
        return baseDamage;
    }

    private float GetAttributeValue(string attributeName)
    {
        // Este método pode ser expandido para suportar mais atributos no futuro
        // Por enquanto, retorna valores baseados em estatísticas existentes
        switch (attributeName.ToLower())
        {
            case "strength":
                return playerStats.Attack * 2;
            case "dexterity":
                return playerStats.MoveSpeed * 3;
            case "intelligence":
                return playerStats.MaxMana / 10;
            case "wisdom":
                return playerStats.MaxMana / 15;
            case "luck":
                return playerStats.Level * 2;
            default:
                return 1f;
        }
    }

    private void ApplyDamage(EnemyStats enemy, float damage)
    {
        // Aplica dano ao inimigo, considerando sua defesa
        enemy.TakeDamage(damage);
        
        // Pode adicionar efeitos especiais com base no tipo de ataque
    }

    private void UseAbility(int abilityIndex)
    {
        if (skillSystem == null) return;
        
        // Verifica se a habilidade está disponível
        if (skillSystem.CanUseSkill(abilityIndex))
        {
            // Determina o tipo de ataque baseado na habilidade
            AttackType attackType = GetAttackTypeFromSkill(abilityIndex);
            
            // Calcula dano baseado no tipo de ataque
            float damage = CalculateDamage(attackType);
            
            // Usa a habilidade
            skillSystem.UseSkill(abilityIndex, damage);
        }
    }

    private AttackType GetAttackTypeFromSkill(int skillIndex)
    {
        // Simplificação - na prática, isso viria dos dados da habilidade
        switch (skillIndex)
        {
            case 0: return AttackType.Melee;
            case 1: return AttackType.Fire; 
            case 2: return AttackType.Ice;
            case 3: return AttackType.Lightning;
            default: return AttackType.Basic;
        }
    }

    private IEnumerator ShowAttackEffect()
    {
        if (attackEffectPrefab != null)
        {
            // Instancia efeito visual
            GameObject effect = Instantiate(attackEffectPrefab, attackPoint.position, Quaternion.identity);
            
            // Destrói após duração
            yield return attackEffectDuration;
            
            if (effect != null)
            {
                Destroy(effect);
            }
        }
    }

    private void PlaySound(AudioClip clip)
    {
        if (audioSource != null && clip != null)
        {
            audioSource.PlayOneShot(clip);
        }
    }

    private void OnDrawGizmosSelected()
    {
        // Visualiza o alcance de ataque no editor
        if (attackPoint == null) return;
        
        Gizmos.color = Color.red;
        Gizmos.DrawWireCircle(attackPoint.position, baseAttackRange);
    }

    // Método para desenhar círculos de gizmos no Unity
    // Este método existirá apenas se a Unity não fornecer DrawWireCircle
    private static void DrawWireCircle(Vector3 position, float radius)
    {
        int segments = 36;
        float angle = 0f;
        Vector3 lastPoint = position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);

        for (int i = 0; i < segments + 1; i++)
        {
            angle = (i * Mathf.PI * 2f) / segments;
            Vector3 nextPoint = position + new Vector3(Mathf.Cos(angle) * radius, Mathf.Sin(angle) * radius, 0);
            Gizmos.DrawLine(lastPoint, nextPoint);
            lastPoint = nextPoint;
        }
    }

    // Métodos públicos para interações externas
    
    public void ForceAttack(Transform target)
    {
        if (Time.time < nextAttackTime) return;
        
        // Direciona para o alvo
        Vector3 direction = target.position - transform.position;
        transform.right = direction.normalized;
        
        // Executa ataque
        PerformBasicAttack();
    }

    public void ApplyWeaponBuff(float damageMultiplier, float duration)
    {
        StartCoroutine(ApplyTemporaryAttackBuff(damageMultiplier, duration));
    }

    private IEnumerator ApplyTemporaryAttackBuff(float multiplier, float duration)
    {
        // Guarda os valores originais
        float originalAttack = playerStats.Attack;
        
        // Aplica buff temporário diretamente nas estatísticas do jogador
        // Nota: Isso presumiria um método na classe PlayerStats para modificar temporariamente o ataque
        
        yield return new WaitForSeconds(duration);
        
        // Restaura valores originais
        // Novamente, isso presumiria um método na classe PlayerStats
    }

    // Método chamado quando o jogador sobe de nível
    public void OnPlayerLevelUp()
    {
        UpdateCombatParameters();
    }
}
