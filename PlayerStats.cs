using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PlayerStats : MonoBehaviour
{
    // Serialized fields para fácil ajuste no Inspector
    [Header("Atributos Base")]
    [SerializeField] private float baseHealth = 100f;
    [SerializeField] private float baseMana = 100f;
    [SerializeField] private float baseAttack = 10f;
    [SerializeField] private float baseDefense = 5f;
    [SerializeField] private float baseMoveSpeed = 5f;
    [SerializeField] private float baseAttackSpeed = 1f;

    [Header("Valores Atuais")]
    [SerializeField] private float currentHealth;
    [SerializeField] private float currentMana;
    [SerializeField] private float currentMoveSpeed;
    
    [Header("Taxas de Regeneração")]
    [SerializeField] private float healthRegenRate = 1f;
    [SerializeField] private float manaRegenRate = 2f;
    [SerializeField] private float regenTickTime = 1f;

    [Header("Level e Experiência")]
    [SerializeField] private int playerLevel = 1;
    
    // Referências a outros componentes
    private ExperienceSystem experienceSystem;
    private PlayerClassType playerClass;
    private PlayerUI playerUI;
    private Animator animator;

    // Propriedades públicas
    public float CurrentHealth => currentHealth;
    public float MaxHealth => baseHealth + (playerLevel * 10f);
    public float CurrentMana => currentMana;
    public float MaxMana => baseMana + (playerLevel * 5f);
    public float Attack => baseAttack + (playerLevel * 2f);
    public float Defense => baseDefense + (playerLevel * 1f);
    public float MoveSpeed => currentMoveSpeed;
    public float AttackSpeed => baseAttackSpeed + (playerLevel * 0.05f);
    public int Level => playerLevel;

    // Eventos
    public delegate void HealthChangedHandler(float currentHealth, float maxHealth);
    public event HealthChangedHandler OnHealthChanged;
    
    public delegate void ManaChangedHandler(float currentMana, float maxMana);
    public event ManaChangedHandler OnManaChanged;
    
    public delegate void LevelChangedHandler(int newLevel);
    public event LevelChangedHandler OnLevelChanged;
    
    public delegate void PlayerDeathHandler();
    public event PlayerDeathHandler OnPlayerDeath;

    private void Awake()
    {
        // Inicializa as referências
        experienceSystem = GetComponent<ExperienceSystem>();
        playerClass = GetComponent<PlayerClassType>();
        playerUI = GetComponent<PlayerUI>();
        animator = GetComponent<Animator>();
        
        // Garante que os componentes necessários existam
        if (experienceSystem == null)
        {
            experienceSystem = gameObject.AddComponent<ExperienceSystem>();
        }
        
        if (playerUI == null)
        {
            Debug.LogWarning("PlayerUI não encontrado no jogador.");
        }
    }

    private void Start()
    {
        // Configura os valores iniciais
        InitializeStats();
        
        // Inicia as regenerações
        StartCoroutine(RegenerateHealth());
        StartCoroutine(RegenerateMana());
        
        // Subscreve aos eventos necessários
        if (experienceSystem != null)
        {
            experienceSystem.OnLevelUp += HandleLevelUp;
        }
        
        // Atualiza a UI inicial
        UpdateUI();
    }

    private void InitializeStats()
    {
        // Ajusta os atributos base de acordo com a classe do jogador
        if (playerClass != null)
        {
            ApplyClassModifiers();
        }
        
        // Define os valores atuais
        currentHealth = MaxHealth;
        currentMana = MaxMana;
        currentMoveSpeed = baseMoveSpeed;
    }

    private void ApplyClassModifiers()
    {
        switch (playerClass.PlayerClass)
        {
            case ClassType.Warrior:
                baseHealth *= 1.3f;
                baseAttack *= 1.2f;
                baseDefense *= 1.2f;
                baseMana *= 0.8f;
                break;
                
            case ClassType.Mage:
                baseMana *= 1.5f;
                baseAttack *= 1.1f;
                baseHealth *= 0.8f;
                manaRegenRate *= 1.5f;
                break;
                
            case ClassType.Archer:
                baseAttackSpeed *= 1.3f;
                baseMoveSpeed *= 1.2f;
                baseAttack *= 1.1f;
                break;
                
            case ClassType.Cleric:
                baseHealth *= 1.1f;
                baseMana *= 1.3f;
                healthRegenRate *= 1.5f;
                break;
        }
    }
    
    private void HandleLevelUp(int newLevel)
    {
        playerLevel = newLevel;
        
        // Regenera completamente ao subir de nível
        currentHealth = MaxHealth;
        currentMana = MaxMana;
        
        // Notifica sobre a mudança de nível
        OnLevelChanged?.Invoke(newLevel);
        
        // Atualiza a UI
        UpdateUI();
    }

    private IEnumerator RegenerateHealth()
    {
        WaitForSeconds wait = new WaitForSeconds(regenTickTime);
        
        while (true)
        {
            yield return wait;
            
            if (currentHealth < MaxHealth)
            {
                ModifyHealth(healthRegenRate);
            }
        }
    }

    private IEnumerator RegenerateMana()
    {
        WaitForSeconds wait = new WaitForSeconds(regenTickTime);
        
        while (true)
        {
            yield return wait;
            
            if (currentMana < MaxMana)
            {
                ModifyMana(manaRegenRate);
            }
        }
    }

    public void ModifyHealth(float amount)
    {
        if (currentHealth <= 0) return;
        
        currentHealth = Mathf.Clamp(currentHealth + amount, 0, MaxHealth);
        
        // Notifica sobre a mudança de saúde
        OnHealthChanged?.Invoke(currentHealth, MaxHealth);
        
        // Atualiza a UI
        UpdateUI();
        
        // Verifica se o jogador morreu
        if (currentHealth <= 0)
        {
            Die();
        }
        else if (amount < 0)
        {
            // Jogador tomou dano
            animator?.SetTrigger("Hit");
        }
    }

    public void ModifyMana(float amount)
    {
        currentMana = Mathf.Clamp(currentMana + amount, 0, MaxMana);
        
        // Notifica sobre a mudança de mana
        OnManaChanged?.Invoke(currentMana, MaxMana);
        
        // Atualiza a UI
        UpdateUI();
    }

    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            ModifyMana(-amount);
            return true;
        }
        
        return false;
    }

    private void Die()
    {
        // Trigger de animação de morte
        animator?.SetTrigger("Death");
        
        // Desativa movimento e ataques
        GetComponent<PlayerClickMovement>()?.enabled = false;
        GetComponent<PlayerAttack>()?.enabled = false;
        
        // Notifica sobre a morte do jogador
        OnPlayerDeath?.Invoke();
        
        // Para as corrotinas de regeneração
        StopAllCoroutines();
        
        // Lógica adicional de morte pode ser adicionada aqui
        Debug.Log("Jogador morreu!");
    }

    public void Revive(float healthPercentage = 0.5f)
    {
        currentHealth = MaxHealth * healthPercentage;
        currentMana = MaxMana * healthPercentage;
        
        // Reativa os componentes
        GetComponent<PlayerClickMovement>()?.enabled = true;
        GetComponent<PlayerAttack>()?.enabled = true;
        
        // Reinicia as regenerações
        StartCoroutine(RegenerateHealth());
        StartCoroutine(RegenerateMana());
        
        // Atualiza a UI
        UpdateUI();
        
        // Trigger de animação de ressurreição (se existir)
        animator?.SetTrigger("Revive");
    }

    private void UpdateUI()
    {
        if (playerUI != null)
        {
            playerUI.UpdateHealthUI(currentHealth, MaxHealth);
            playerUI.UpdateManaUI(currentMana, MaxMana);
            playerUI.UpdateLevelUI(playerLevel);
        }
    }

    // Métodos para buff/debuff temporários
    public IEnumerator ApplySpeedModifier(float modifier, float duration)
    {
        float originalSpeed = currentMoveSpeed;
        currentMoveSpeed *= modifier;
        
        yield return new WaitForSeconds(duration);
        
        currentMoveSpeed = originalSpeed;
    }

    // Método para salvar o estado do jogador (pode ser expandido)
    public Dictionary<string, object> SavePlayerData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            {"currentHealth", currentHealth},
            {"currentMana", currentMana},
            {"playerLevel", playerLevel}
        };
        
        return data;
    }

    // Método para carregar o estado do jogador (pode ser expandido)
    public void LoadPlayerData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("currentHealth"))
            currentHealth = (float)data["currentHealth"];
            
        if (data.ContainsKey("currentMana"))
            currentMana = (float)data["currentMana"];
            
        if (data.ContainsKey("playerLevel"))
            playerLevel = (int)data["playerLevel"];
            
        UpdateUI();
    }
}
