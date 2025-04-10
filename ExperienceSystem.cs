using UnityEngine;

public class ExperienceSystem : MonoBehaviour
{
    public int level = 1;
    public int currentXP = 0;
    public int xpToNextLevel = 100;

    public int attributePoints = 0;
    
    private PlayerStats playerStats;
    
    private void Awake()
    {
        playerStats = GetComponent<PlayerStats>();
    }
    
    private void Start()
    {
        // Certifica que o level inicial está sincronizado
        if (playerStats != null)
        {
            playerStats.level = level;
        }
    }

    public void GainXP(int amount)
    {
        currentXP += amount;

        while (currentXP >= xpToNextLevel)
        {
            currentXP -= xpToNextLevel;
            LevelUp();
        }
    }

    private void LevelUp()
    {
        level++;
        xpToNextLevel = Mathf.RoundToInt(xpToNextLevel * 1.25f);
        attributePoints += 5;

        Debug.Log($"Subiu para o nível {level}! Ganhou 5 pontos de atributo.");

        // Informa o PlayerStats para atualizar atributos
        if (playerStats != null)
        {
            playerStats.OnLevelUp();
            // Garante que o level está sincronizado
            playerStats.level = level;
        }
    }

    public bool SpendPoint(string attribute)
    {
        if (attributePoints <= 0) return false;

        var stats = GetComponent<PlayerStats>();
        if (stats == null) return false;

        bool result = stats.ApplyAttributeIncrease(attribute);
        if (result) 
        {
            attributePoints--;
            Debug.Log($"Ponto gasto em {attribute}. Pontos restantes: {attributePoints}");
        }

        return result;
    }
}