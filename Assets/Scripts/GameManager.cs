using UnityEngine;
using TMPro;
using UnityEngine.Events;

public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }
    
    [Header("Game Settings")]
    public int startingHealth = 20;
    public int startingCurrency = 500;
    public int TotalWaves = 3;
    public int baseEnemiesPerWave = 5;
    
    // Events for UI to listen to
    public delegate void CurrencyChangedEvent(int newAmount);
    public event CurrencyChangedEvent OnCurrencyChanged;
    
    public delegate void HealthChangedEvent(int current, int max);
    public event HealthChangedEvent OnHealthChanged;
    
    public delegate void WaveChangedEvent(int current, int total, int enemies);
    public event WaveChangedEvent OnWaveChanged;
    
    private int currentHealth;
    private int currentWave = 1;
    private int remainingEnemies;
    private TurretManager turretManager;
    
    // Properties for external access
    public int MaxBaseHealth => startingHealth;
    public int CurrentBaseHealth => currentHealth;
    public int CurrentWave => currentWave;
    public int RemainingEnemies => remainingEnemies;
    public int CurrentCurrency => _currency;
    public int EnemiesPerWave => baseEnemiesPerWave + (5 * (currentWave - 1));
    
    [Header("Currency")]
    public TextMeshProUGUI currencyText;
    public UnityEvent<int> OnCurrencyChangedUnity;
    
    private int _currency;
    public int Currency 
    { 
        get { return _currency; } 
        private set 
        { 
            _currency = value; 
            OnCurrencyChanged?.Invoke(_currency);
            OnCurrencyChangedUnity?.Invoke(_currency);
            if (currencyText != null)
                currencyText.text = $"{_currency} Cals";
                
            // Only update TurretManager UI when Currency changes
            if (turretManager != null)
            {
                turretManager.UpdateCurrencyUI();
            }
        } 
    }
    
    void Awake()
    {
        // Singleton pattern
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }
        Instance = this;
        
        // Don't destroy when loading new scenes
        //DontDestroyOnLoad(gameObject);
        
        // Initialize game state
        currentHealth = startingHealth;
        currentWave = 1;  // Explicitly set to wave 1
        remainingEnemies = EnemiesPerWave;
    }
    
    void Start()
    {
        turretManager = FindObjectOfType<TurretManager>();
        
        // Set starting currency - do this first to initialize the value
        Currency = startingCurrency;
        
        // Initialize by triggering events for UI elements
        if (OnHealthChanged != null)
            OnHealthChanged(currentHealth, startingHealth);
            
        if (OnCurrencyChanged != null)
            OnCurrencyChanged(Currency);
            
        if (OnWaveChanged != null)
            OnWaveChanged(currentWave, TotalWaves, remainingEnemies);
        
        Debug.Log($"GameManager initialized: Wave {currentWave}, {remainingEnemies} enemies, {startingHealth} health");
    }
    
    public void StartNextWave()
    {
        // Only increment if we're not already on the last wave
        if (currentWave < TotalWaves)
        {
            currentWave++;
            
            // Reset the remainingEnemies counter to the correct value for the new wave
            remainingEnemies = EnemiesPerWave;
            
            Debug.Log($"GameManager starting wave {currentWave} with {remainingEnemies} enemies");
            
            // Notify UI of wave changes
            if (OnWaveChanged != null)
                OnWaveChanged(currentWave, TotalWaves, remainingEnemies);
                
            // The EnemySpawner will handle the actual spawning
        }
        else
        {
            WinGame();
        }
    }
    
    public bool CanAfford(int amount)
    {
        return Currency >= amount;
    }
    
    public bool SpendCurrency(int amount)
    {
        if (!CanAfford(amount)) return false;
        
        Currency -= amount;
        Debug.Log($"Spent {amount} Cals. New balance: {Currency} Cals");
        return true;
    }
    
    public void AddCurrency(int amount)
    {
        Currency += amount;
        Debug.Log($"Added {amount} Cals. New balance: {Currency} Cals");
    }
    
    void GameOver()
    {
        Debug.Log("Game Over!");
        // Get GameUI and show lose screen
        GameUI gameUI = FindObjectOfType<GameUI>();
        if (gameUI != null)
        {
            gameUI.ShowLoseScreen();
        }
    }
    
    void WinGame()
    {
        Debug.Log("Victory!");
        // Get GameUI and show win screen
        GameUI gameUI = FindObjectOfType<GameUI>();
        if (gameUI != null)
        {
            gameUI.ShowWinScreen();
        }
    }
    
    // Called when an enemy reaches the end - no currency reward
    public void EnemyReachedEnd()
    {
        // Just decrease remaining enemies count (no currency reward)
        remainingEnemies--;
        
        Debug.Log($"Enemy reached end! {remainingEnemies} enemies remaining in wave {currentWave}");
        
        // Notify UI of wave changes
        if (OnWaveChanged != null)
            OnWaveChanged(currentWave, TotalWaves, remainingEnemies);

        if (remainingEnemies <= 0 && currentWave >= TotalWaves && currentHealth > 0)
        {
            WinGame();
        }
    }
    
    public void EnemyDefeated()
    {
        // Award currency for defeating an enemy
        AddCurrency(25);
        remainingEnemies--;
        
        Debug.Log($"Enemy defeated! {remainingEnemies} enemies remaining in wave {currentWave}");
        
        // Notify UI of wave changes
        if (OnWaveChanged != null)
            OnWaveChanged(currentWave, TotalWaves, remainingEnemies);
        
        if (remainingEnemies <= 0 && currentWave >= TotalWaves)
        {
            WinGame();
        }
    }
    
    public void TakeDamage(int damage)
    {
        if (damage <= 0)
        {
            Debug.LogWarning($"GameManager received invalid damage amount: {damage}");
            return;
        }
        
        int previousHealth = currentHealth;
        currentHealth -= damage;
        
        // Clamp health to prevent negative values
        currentHealth = Mathf.Max(0, currentHealth);
        
        Debug.Log($"Base health changed: {previousHealth} → {currentHealth} (-{damage}) | {currentHealth}/{startingHealth} = {(float)currentHealth/startingHealth*100:F1}% remaining");
        
        // Notify UI
        if (OnHealthChanged != null)
            OnHealthChanged(currentHealth, startingHealth);
        
        if (currentHealth <= 0)
        {
            Debug.Log("Base health depleted! Game Over!");
            GameOver();
        }
    }
}