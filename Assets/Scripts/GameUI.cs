using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System;

public class GameUI : MonoBehaviour
{
    [Header("Health Bar")]
    public RectTransform healthBar;       
    public Image healthBarImage;          
    
    [Header("Text Elements")]
    public TextMeshProUGUI waveText;       
    public TextMeshProUGUI enemyCountText; 
    public TextMeshProUGUI currencyText; 
    
    [Header("Win/Lose Screens")]
    public GameObject winScreen;
    public GameObject loseScreen;
    public GameObject restart;
    public GameObject next;
    
    [Header("Health Bar Settings")]
    public float originalWidth = 500f;  
    public Color healthyColor = Color.green;
    public Color mediumColor = Color.yellow;
    public Color lowHealthColor = Color.red;
    public float mediumHealthThreshold = 0.66f;
    public float lowHealthThreshold = 0.33f;

    private GameManager gameManager;
    public static bool isPlaying = true;

    void Start()
    {
        isPlaying = true;
        gameManager = GameManager.Instance;
        
        // Initialize win/lose screens
        if (winScreen != null) winScreen.SetActive(false);
        if (loseScreen != null) loseScreen.SetActive(false);
        
        // Subscribe to GameManager events
        if (gameManager != null)
        {
            gameManager.OnHealthChanged += UpdateHealth;
            gameManager.OnCurrencyChanged += UpdateCurrency;
            gameManager.OnWaveChanged += UpdateWaveInfo;
            
            // Force initial update
            UpdateHealth(gameManager.CurrentBaseHealth, gameManager.MaxBaseHealth);
            UpdateCurrency(gameManager.CurrentCurrency);
            UpdateWaveInfo(gameManager.CurrentWave, gameManager.TotalWaves, gameManager.RemainingEnemies);
        }
        else
        {
            Debug.LogError("GameUI: No GameManager instance found!");
        }
    }
    
    private void OnDestroy()
    {
        // Unsubscribe from events when destroyed
        if (gameManager != null)
        {
            gameManager.OnHealthChanged -= UpdateHealth;
            gameManager.OnCurrencyChanged -= UpdateCurrency;
            gameManager.OnWaveChanged -= UpdateWaveInfo;
        }
    }
    
    public void UpdateHealth(int currentHealth, int maxHealth)
    {
        if (healthBar == null)
        {
            Debug.LogError("GameUI: healthBar is null!");
            return;
        }
        
        if (healthBarImage == null)
        {
            Debug.LogError("GameUI: healthBarImage is null!");
            return;
        }

        float healthPercent = (float)currentHealth / maxHealth;
        
        // Update health bar size
        float newWidth = originalWidth * healthPercent;
        healthBar.SetSizeWithCurrentAnchors(RectTransform.Axis.Horizontal, newWidth);
        
        Debug.Log($"GameUI: Updating health bar - {currentHealth}/{maxHealth} = {healthPercent*100:F1}% - Width: {newWidth:F1}px");
        
        // Update health bar color based on health percentage
        Color newColor;
        if (healthPercent > mediumHealthThreshold)
        {
            newColor = healthyColor;
            healthBarImage.color = newColor;
        }
        else if (healthPercent > lowHealthThreshold)
        {
            float t = (healthPercent - lowHealthThreshold) / (mediumHealthThreshold - lowHealthThreshold);
            newColor = Color.Lerp(mediumColor, healthyColor, t);
            healthBarImage.color = newColor;
        }
        else
        {
            float t = healthPercent / lowHealthThreshold;
            newColor = Color.Lerp(lowHealthColor, mediumColor, t);
            healthBarImage.color = newColor;
        }
        
        Debug.Log($"GameUI: Health bar color set to {ColorToHex(newColor)} based on health percent {healthPercent*100:F1}%");
    }
    
    // Helper method to convert a color to hex for debugging
    private string ColorToHex(Color color)
    {
        return $"#{ColorUtility.ToHtmlStringRGBA(color)}";
    }
    
    public void UpdateCurrency(int amount)
    {
        if (currencyText != null)
        {
            currencyText.text = $"{amount} Calories";
        }
    }

    public void UpdateWaveInfo(int currentWave, int totalWaves, int remainingEnemies)
    {
        if (waveText != null)
        {
            waveText.text = $"Waves: {currentWave}/{totalWaves}";
        }
        
        if (enemyCountText != null)
        {
            int enemiesPerWave = gameManager != null ? gameManager.EnemiesPerWave : 0;
            
            // Ensure we don't show negative enemy counts
            int displayedRemaining = Mathf.Max(0, remainingEnemies);
            
            enemyCountText.text = $"Enemies: {displayedRemaining}/{enemiesPerWave}";
            
            Debug.Log($"UI updated: Wave {currentWave}/{totalWaves}, Enemies {displayedRemaining}/{enemiesPerWave}");
        }
    }

    public void ShowWinScreen()
    {
        isPlaying = false;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (winScreen != null){
            winScreen.SetActive(true);
            if(next != null){
                next.SetActive(true);
            }
        }
    }

    public void ShowLoseScreen()
    {
        isPlaying = false;
        Time.timeScale = 0;
        Cursor.visible = true;
        Cursor.lockState = CursorLockMode.None;
        if (loseScreen != null){
            loseScreen.SetActive(true);        
            if(restart != null){
                restart.SetActive(true);
            }    
        }
    }
}