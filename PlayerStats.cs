using UnityEngine;

public class PlayerStats : MonoBehaviour
{
    [Header("Atributos Base")]
    public int level = 1;
    public int strength = 5;
    public int agility = 5;
    public int dexterity = 5;
    public int intelligence = 5;
    public int luck = 5;
    public int vitality = 5;

    [Header("Vida e Mana")]
    public float maxHealth;
    public float currentHealth;

    public float maxMana;
    public float currentMana;

    [Header("Regeneração")]
    public float manaRegenRate = 2f;
    private float manaRegenCooldown = 0f;

    private bool isDead = false;

    [Header("Classe do Jogador")]
    public PlayerClassType currentClass = PlayerClassType.Aprendiz;
    
    // Referência para o ExperienceSystem
    private ExperienceSystem expSystem;

    private void Awake()
    {
        expSystem = GetComponent<ExperienceSystem>();
    }

    private void Start()
    {
        CalculateDerivedStats();
        currentHealth = maxHealth;
        currentMana = maxMana;
        
        // Sincroniza o level entre os componentes
        if (expSystem != null)
        {
            level = expSystem.level;
        }
    }

    private void Update()
    {
        // Sincroniza o level entre os componentes
        if (expSystem != null && level != expSystem.level)
        {
            level = expSystem.level;
        }
        
        if (manaRegenCooldown > 0)
        {
            manaRegenCooldown -= Time.deltaTime;
        }
        else
        {
            RegenerateMana();
        }
    }

    public void TakeDamage(float amount)
    {
        currentHealth = Mathf.Clamp(currentHealth - amount, 0f, maxHealth);
        if (currentHealth <= 0f)
        {
            Die();
        }
    }

    public bool UseMana(float amount)
    {
        if (currentMana >= amount)
        {
            currentMana -= amount;
            manaRegenCooldown = 0.5f; // Pequeno cooldown após usar mana
            return true;
        }
        return false;
    }

    private void RegenerateMana()
    {
        if (currentMana < maxMana)
        {
            currentMana += manaRegenRate * Time.deltaTime;
            currentMana = Mathf.Clamp(currentMana, 0, maxMana);
        }
    }
    
    public void TryChangeClass(PlayerClassType desiredClass)
    {
        if (CanChangeTo(desiredClass))
        {
            currentClass = desiredClass;
            Debug.Log("Classe alterada para: " + desiredClass);
            OnClassChanged();
        }
        else
        {
            Debug.Log("Requisitos não atendidos para mudar para: " + desiredClass);
        }
    }

    private bool CanChangeTo(PlayerClassType desiredClass)
    {
        switch (desiredClass)
        {
            case PlayerClassType.Guerreiro:
                return level >= 5 && strength >= 10;
            case PlayerClassType.Mago:
                return level >= 5 && intelligence >= 10;
            case PlayerClassType.Arqueiro:
                return level >= 5 && dexterity >= 10;
            case PlayerClassType.Ladino:
                return level >= 5 && agility >= 10;
            case PlayerClassType.Clérigo:
                return level >= 5 && intelligence >= 8 && luck >= 5;
            default:
                return false;
        }
    }

    private void OnClassChanged()
    {
        // Aqui você pode aplicar bônus ou desbloquear skills específicas
        switch (currentClass)
        {
            case PlayerClassType.Guerreiro:
                strength += 5;
                maxHealth += 30;
                break;
            case PlayerClassType.Mago:
                intelligence += 5;
                maxMana += 50;
                break;
            case PlayerClassType.Arqueiro:
                dexterity += 5;
                agility += 3;
                break;
            case PlayerClassType.Ladino:
                agility += 5;
                luck += 3;
                break;
            case PlayerClassType.Clérigo:
                intelligence += 3;
                maxMana += 30;
                break;
        }

        currentHealth = maxHealth;
        currentMana = maxMana;
    }

    public void Die()
    {
        if (isDead) return;

        isDead = true;
        Debug.Log("Player morreu!");
        // Aqui você pode desativar controles, mostrar tela de morte, etc.
    }

    public bool IsDead()
    {
        return isDead;
    }

    private void CalculateDerivedStats()
    {
        maxHealth = 50 + (vitality * 15);
        maxMana = 30 + (intelligence * 10);
    }

    public void OnLevelUp()
    {
        level++;
        
        // Guarda o valor atual da vida e mana antes de recalcular
        float currentHealthValue = currentHealth;
        float currentManaValue = currentMana;
        
        CalculateDerivedStats();
        
        // Define os valores atuais para os valores cheios no levelup
        currentHealth = maxHealth;
        currentMana = maxMana;
    }
    
    public bool ApplyAttributeIncrease(string attribute)
    {
        // Guarda o valor atual da mana antes da atualização
        float currentManaValue = currentMana;
        float oldMaxMana = maxMana;
        
        switch (attribute.ToLower())
        {
            case "forca":
            case "strength":
                strength++;
                break;
            case "agilidade":
            case "agility":
                agility++;
                break;
            case "vitalidade":
            case "vitality":
                vitality++;
                break;
            case "destreza":
            case "dexterity":
                dexterity++;
                break;
            case "inteligencia":
            case "intelligence":
                intelligence++;
                break;
            case "sorte":
            case "luck":
                luck++;
                break;
            default:
                return false;
        }   

        // Recalcula stats derivados
        CalculateDerivedStats();
        
        // Mantém a mesma mana atual para inteligência (não aumenta automaticamente)
        if (attribute.ToLower() == "intelligence" || attribute.ToLower() == "inteligencia")
        {
            // Mantém o mesmo valor de mana atual, a menos que exceda o novo máximo
            currentMana = Mathf.Min(currentManaValue, maxMana);
            
            //Debug.Log($"Inteligência aumentada. maxMana anterior: {oldMaxMana}, nova maxMana: {maxMana}, currentMana mantida em: {currentMana}");
        }
        
        return true;
    }
}