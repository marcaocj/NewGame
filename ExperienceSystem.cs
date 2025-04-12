using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

public class ExperienceSystem : MonoBehaviour
{
    [Header("Configurações de XP")]
    [SerializeField] private int currentExperience = 0;
    [SerializeField] private int currentLevel = 1;
    [SerializeField] private int maxLevel = 50;
    
    [Header("Curva de Níveis")]
    [SerializeField] private AnimationCurve levelCurve;
    [SerializeField] private int baseXpForNextLevel = 100;
    [SerializeField] private float levelExponentialFactor = 1.5f;
    
    [Header("Multiplicadores de XP")]
    [SerializeField] private float questXpMultiplier = 1.0f;
    [SerializeField] private float combatXpMultiplier = 1.0f;
    [SerializeField] private float explorationXpMultiplier = 1.0f;
    
    [Header("Feedback Visual")]
    [SerializeField] private GameObject levelUpEffectPrefab;
    [SerializeField] private AudioClip levelUpSound;
    [SerializeField] private Color xpGainTextColor = new Color(0.5f, 0.8f, 1f);
    
    // Cache de requisitos de XP
    private Dictionary<int, int> xpRequirementCache = new Dictionary<int, int>();
    
    // Referências a outros componentes
    private PlayerStats playerStats;
    private PlayerUI playerUI;
    private AudioSource audioSource;
    
    // Eventos públicos
    public delegate void LevelUpHandler(int newLevel);
    public event LevelUpHandler OnLevelUp;
    
    public delegate void ExperienceGainHandler(int amount, ExperienceSource source);
    public event ExperienceGainHandler OnExperienceGained;
    
    public enum ExperienceSource
    {
        Combat,
        Quest,
        Exploration,
        Crafting,
        Training,
        Other
    }
    
    // Propriedades públicas
    public int CurrentExperience => currentExperience;
    public int CurrentLevel => currentLevel;
    public int ExperienceToNextLevel => GetXpRequiredForLevel(currentLevel + 1) - currentExperience;
    public float LevelProgress => (float)currentExperience / GetXpRequiredForLevel(currentLevel + 1);
    
    private void Awake()
    {
        // Inicializa componentes
        playerStats = GetComponent<PlayerStats>();
        playerUI = GetComponent<PlayerUI>();
        audioSource = GetComponent<AudioSource>();
        
        if (audioSource == null)
        {
            audioSource = gameObject.AddComponent<AudioSource>();
        }
        
        // Inicializa a curva de nível, caso esteja vazia
        if (levelCurve.length == 0)
        {
            InitializeDefaultLevelCurve();
        }
        
        // Pré-calcula alguns requisitos de XP para melhor performance
        PrecalculateXpRequirements();
    }
    
    private void Start()
    {
        // Atualiza a UI
        UpdateUI();
    }
    
    private void InitializeDefaultLevelCurve()
    {
        // Cria uma curva de nível padrão se nenhuma for fornecida
        levelCurve = new AnimationCurve();
        
        // Adiciona alguns keyframes para criar uma curva exponencial suave
        levelCurve.AddKey(0, 0);
        levelCurve.AddKey(0.2f, 0.1f);
        levelCurve.AddKey(0.4f, 0.3f);
        levelCurve.AddKey(0.6f, 0.6f);
        levelCurve.AddKey(0.8f, 0.8f);
        levelCurve.AddKey(1.0f, 1.0f);
        
        // Ajusta as tangentes para uma curva mais suave
        for (int i = 0; i < levelCurve.length; i++)
        {
            AnimationUtility.SetKeyLeftTangentMode(levelCurve, i, AnimationUtility.TangentMode.Auto);
            AnimationUtility.SetKeyRightTangentMode(levelCurve, i, AnimationUtility.TangentMode.Auto);
        }
    }
    
    private void PrecalculateXpRequirements()
    {
        // Pré-calcula os requisitos de XP para cada nível até o máximo
        for (int level = 1; level <= maxLevel; level++)
        {
            int xpRequired = CalculateXpRequiredForLevel(level);
            xpRequirementCache[level] = xpRequired;
        }
    }
    
    private int CalculateXpRequiredForLevel(int level)
    {
        if (level <= 1) return 0;
        
        // Usa a curva de nível para ajustar a progressão
        float normalizedLevel = (float)(level - 1) / maxLevel;
        float curveValue = levelCurve.Evaluate(normalizedLevel);
        
        // Calcula o XP necessário usando uma fórmula exponencial ajustada pela curva
        return Mathf.RoundToInt(baseXpForNextLevel * Mathf.Pow(level, levelExponentialFactor) * (1 + curveValue));
    }
    
    private int GetXpRequiredForLevel(int level)
    {
        // Retorna do cache se disponível
        if (xpRequirementCache.TryGetValue(level, out int xpRequired))
        {
            return xpRequired;
        }
        
        // Calcula se não estiver no cache
        xpRequired = CalculateXpRequiredForLevel(level);
        xpRequirementCache[level] = xpRequired;
        return xpRequired;
    }
    
    public void AddExperience(int amount, ExperienceSource source = ExperienceSource.Combat)
    {
        if (currentLevel >= maxLevel) return;
        
        // Aplica multiplicadores baseados na fonte
        float multiplier = GetSourceMultiplier(source);
        int adjustedAmount = Mathf.RoundToInt(amount * multiplier);
        
        // Adiciona a experiência
        currentExperience += adjustedAmount;
        
        // Dispara evento de ganho de experiência
        OnExperienceGained?.Invoke(adjustedAmount, source);
        
        // Mostra feedback visual
        ShowXpGainFeedback(adjustedAmount, source);
        
        // Verifica se subiu de nível
        CheckLevelUp();
        
        // Atualiza a UI
        UpdateUI();
    }
    
    private float GetSourceMultiplier(ExperienceSource source)
    {
        switch (source)
        {
            case ExperienceSource.Combat:
                return combatXpMultiplier;
                
            case ExperienceSource.Quest:
                return questXpMultiplier;
                
            case ExperienceSource.Exploration:
                return explorationXpMultiplier;
                
            case ExperienceSource.Crafting:
                // Pode ser expandido com mais multiplicadores no futuro
                return 0.8f;
                
            case ExperienceSource.Training:
                return 0.5f;
                
            default:
                return 1.0f;
        }
    }
    
    private void CheckLevelUp()
    {
        int xpForNextLevel = GetXpRequiredForLevel(currentLevel + 1);
        
        // Enquanto tiver XP suficiente para subir de nível
        while (currentExperience >= xpForNextLevel && currentLevel < maxLevel)
        {
            // Sobe de nível
            currentLevel++;
            
            // Dispara eventos de nível
            OnLevelUp?.Invoke(currentLevel);
            
            // Aplica efeitos visuais e sonoros
            PlayLevelUpEffects();
            
            // Atualiza requisito para o próximo nível
            xpForNextLevel = GetXpRequiredForLevel(currentLevel + 1);
        }
    }
    
    private void PlayLevelUpEffects()
    {
        // Efeito visual de subida de nível
        if (levelUpEffectPrefab != null)
        {
            GameObject effect = Instantiate(levelUpEffectPrefab, transform.position, Quaternion.identity);
            Destroy(effect, 3f);
        }
        
        // Som de subida de nível
        if (audioSource != null && levelUpSound != null)
        {
            audioSource.PlayOneShot(levelUpSound);
        }
        
        // Mais efeitos podem ser adicionados aqui
    }
    
    private void ShowXpGainFeedback(int amount, ExperienceSource source)
    {
        // Esta função pode ser expandida para mostrar texto flutuante, etc.
        if (playerUI != null)
        {
            string sourceText = source == ExperienceSource.Combat ? "combate" : 
                               source == ExperienceSource.Quest ? "missão" : 
                               source == ExperienceSource.Exploration ? "exploração" : 
                               source.ToString();
            
            playerUI.ShowFloatingText($"+{amount} XP ({sourceText})", transform.position + Vector3.up, xpGainTextColor);
        }
    }
    
    private void UpdateUI()
    {
        if (playerUI != null)
        {
            playerUI.UpdateExperienceUI(currentExperience, GetXpRequiredForLevel(currentLevel + 1), currentLevel);
        }
    }
    
    // Método para distribuir automaticamente pontos de atributo 
    // (para ser expandido com um sistema de atributos)
    public void AutoDistributeAttributePoints(int points)
    {
        if (playerStats == null) return;
        
        // Implementação para distribuição de pontos de atributo pode ser adicionada aqui
        // quando você criar um sistema de atributos
    }
    
    // Métodos públicos para consultas externas
    
    public int GetTotalExperienceForLevel(int level)
    {
        if (level <= 1) return 0;
        return GetXpRequiredForLevel(level);
    }
    
    public float GetExperienceMultiplier(ExperienceSource source)
    {
        return GetSourceMultiplier(source);
    }
    
    public void SetExperienceMultiplier(ExperienceSource source, float multiplier)
    {
        switch (source)
        {
            case ExperienceSource.Combat:
                combatXpMultiplier = multiplier;
                break;
                
            case ExperienceSource.Quest:
                questXpMultiplier = multiplier;
                break;
                
            case ExperienceSource.Exploration:
                explorationXpMultiplier = multiplier;
                break;
        }
    }
    
    // Métodos para testes e cheats (podem ser removidos na versão final)
    
    public void AddLevel(int levels = 1)
    {
        for (int i = 0; i < levels; i++)
        {
            int xpNeeded = GetXpRequiredForLevel(currentLevel + 1) - currentExperience;
            AddExperience(xpNeeded + 1, ExperienceSource.Other);
        }
    }
    
    public void SetLevel(int level)
    {
        if (level < 1 || level > maxLevel) return;
        
        // Reseta XP
        currentExperience = 0;
        currentLevel = 1;
        
        // Adiciona níveis
        AddLevel(level - 1);
    }
    
    // Métodos para salvar/carregar
    
    public Dictionary<string, object> SaveExperienceData()
    {
        Dictionary<string, object> data = new Dictionary<string, object>
        {
            {"currentLevel", currentLevel},
            {"currentExperience", currentExperience},
            {"combatXpMultiplier", combatXpMultiplier},
            {"questXpMultiplier", questXpMultiplier},
            {"explorationXpMultiplier", explorationXpMultiplier}
        };
        
        return data;
    }
    
    public void LoadExperienceData(Dictionary<string, object> data)
    {
        if (data.ContainsKey("currentLevel"))
            currentLevel = (int)data["currentLevel"];
            
        if (data.ContainsKey("currentExperience"))
            currentExperience = (int)data["currentExperience"];
            
        if (data.ContainsKey("combatXpMultiplier"))
            combatXpMultiplier = (float)data["combatXpMultiplier"];
            
        if (data.ContainsKey("questXpMultiplier"))
            questXpMultiplier = (float)data["questXpMultiplier"];
            
        if (data.ContainsKey("explorationXpMultiplier"))
            explorationXpMultiplier = (float)data["explorationXpMultiplier"];
            
        // Atualiza a UI após carregar
        UpdateUI();
    }
}

// Extensão do UnityEditor para acessar o modo de tangente da AnimationCurve
#if UNITY_EDITOR
public static class AnimationUtility
{
    public enum TangentMode
    {
        Auto,
        Linear,
        Constant
    }
    
    public static void SetKeyLeftTangentMode(AnimationCurve curve, int keyIndex, TangentMode mode)
    {
        // Implementação simplificada para este exemplo
        // Na prática, usaria UnityEditor.AnimationUtility
    }
    
    public static void SetKeyRightTangentMode(AnimationCurve curve, int keyIndex, TangentMode mode)
    {
        // Implementação simplificada para este exemplo
        // Na prática, usaria UnityEditor.AnimationUtility
    }
}
#endif
