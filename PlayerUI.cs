using UnityEngine;
using UnityEngine.UI;

public class PlayerUI : MonoBehaviour
{
    public PlayerStats playerStats;
    public ExperienceSystem xpSystem;

    public Slider healthBar;
    public Slider manaBar;
    public Slider xpBar;

    public Text strengthText;
    public Text agilityText;
    public Text dexterityText;
    public Text intelligenceText;
    public Text vitalityText;
    public Text luckText;
    public Text pointsAvailableText;

    private void Update()
    {
        healthBar.maxValue = playerStats.maxHealth;
        healthBar.value = playerStats.currentHealth;

        manaBar.maxValue = playerStats.maxMana;
        manaBar.value = playerStats.currentMana;

        xpBar.maxValue = xpSystem.xpToNextLevel;
        xpBar.value = xpSystem.currentXP;

        pointsAvailableText.text = $"Pontos: {xpSystem.attributePoints}";

        strengthText.text = $"Força: {playerStats.strength}";
        agilityText.text = $"Agilidade: {playerStats.agility}";
        dexterityText.text = $"Destreza: {playerStats.dexterity}";
        intelligenceText.text = $"Inteligência: {playerStats.intelligence}";
        vitalityText.text = $"Vitalidade: {playerStats.vitality}";
        luckText.text = $"Sorte: {playerStats.luck}";
    }

    public void AddStrength() => TryAdd("strength");
    public void AddAgility() => TryAdd("agility");
    public void AddDexterity() => TryAdd("dexterity");
    public void AddIntelligence() => TryAdd("intelligence");
    public void AddVitality() => TryAdd("vitality");
    public void AddLuck() => TryAdd("luck");

    private void TryAdd(string attr)
    {
        xpSystem?.SpendPoint(attr);
    }
    
}
