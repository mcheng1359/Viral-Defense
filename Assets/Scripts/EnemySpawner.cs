using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class EnemySpawner : MonoBehaviour
{
    [Header("Spawning Settings")]
    public int seed = 12345;
    public GameObject[] enemyPrefabs;
    public GameObject bossPrefab;
    public Transform spawnPoint;
    public float timeBetweenSpawns = 2f;
    
    [Header("Wave Control")]
    public float delayBetweenWaves = 5f;
    
    private int currentWave = 0;
    private int enemiesPerWave;
    private int remainingEnemiesInWave;
    private bool isSpawning = false;
    private GameManager gameManager;
    private List<GameObject> activeEnemies = new List<GameObject>();
    private System.Random seededRandom;
    
    void Start()
    {
        seededRandom = new System.Random(seed);
        gameManager = GameManager.Instance;
        
        if (gameManager == null)
        {
            Debug.LogError("GameManager instance not found");
            return;
        }
        
        // If no spawn point is assigned, use this object's position
        if (spawnPoint == null)
            spawnPoint = transform;
        
        // Listen to GameManager's wave events
        gameManager.OnWaveChanged += HandleWaveChanged;
        
        // Initialize our wave counter to match GameManager (should be 1)
        currentWave = gameManager.CurrentWave;
        
        // Start the first wave
        StartCoroutine(DelayedStart());
    }
    
    IEnumerator DelayedStart()
    {
        // Short delay before starting first wave
        yield return new WaitForSeconds(2f);
        
        // For wave 1, we don't need to increment the wave counter
        if (currentWave == 1)
        {
            // Get the number of enemies from GameManager based on current wave
            enemiesPerWave = gameManager.EnemiesPerWave;
            remainingEnemiesInWave = enemiesPerWave;
            
            // Just start spawning the first wave directly
            StartCoroutine(SpawnWave());
        }
        else
        {
            // For any other case, use the normal wave starting mechanism
            StartNextWave();
        }
    }
    
    void HandleWaveChanged(int currentWave, int totalWaves, int remainingEnemies)
    {
        // This is just to keep track of UI updates from GameManager
        Debug.Log($"Wave changed: {currentWave}/{totalWaves}, {remainingEnemies} enemies remaining");
    }
    
    public void EnemyKilled(GameObject enemy, bool wasKilled = true)
    {
        // Remove from our active enemies list
        if (activeEnemies.Contains(enemy))
        {
            activeEnemies.Remove(enemy);
        }
        
        // Notify GameManager - only if the enemy was actually killed
        if (gameManager != null && wasKilled)
        {
            gameManager.EnemyDefeated();
        }
        else if (!wasKilled)
        {
            // If the enemy reached the end, we still need to decrement the counter
            // but without giving currency/rewards
            gameManager.EnemyReachedEnd();
        }
        
        // Check if all enemies in this wave are dead
        CheckWaveComplete();
    }
    
    void CheckWaveComplete()
    {
        // Count active enemies in the scene
        int activeEnemyCount = activeEnemies.Count;
        
        if (activeEnemyCount == 0 && !isSpawning)
        {
            Debug.Log($"All enemies defeated. Starting next wave after delay.");
            StartCoroutine(StartNextWaveAfterDelay());
        }
    }
    
    IEnumerator StartNextWaveAfterDelay()
    {
        yield return new WaitForSeconds(delayBetweenWaves);
        
        // Check again to make sure we're still out of enemies
        if (activeEnemies.Count == 0 && !isSpawning && currentWave < gameManager.TotalWaves)
        {
            StartNextWave();
        }
    }
    
    public void StartNextWave()
    {
        if (isSpawning) return;
        
        // Don't increment our own wave counter, let GameManager do it
        // Tell GameManager to start the next wave - this will update UI and state correctly
        if (gameManager != null && currentWave < gameManager.TotalWaves)
        {
            // For wave transitions after wave 1
            if (currentWave >= 1)
            {
                // Call GameManager's StartNextWave method
                gameManager.StartNextWave();
            }
            
            // Now sync our local wave counter with GameManager
            currentWave = gameManager.CurrentWave;
            
            // Get the number of enemies from GameManager based on current wave
            enemiesPerWave = gameManager.EnemiesPerWave;
            remainingEnemiesInWave = enemiesPerWave;
            
            Debug.Log($"EnemySpawner: Starting wave {currentWave} with {remainingEnemiesInWave} enemies");
            
            StartCoroutine(SpawnWave());
        }
        else if (currentWave >= gameManager.TotalWaves)
        {
            Debug.Log("All waves completed!");
        }
    }
    
    IEnumerator SpawnWave()
    {
        isSpawning = true;
        
        Debug.Log($"Starting Wave {currentWave}/{gameManager.TotalWaves} - Spawning {remainingEnemiesInWave} enemies");
        
        // Wait for a short delay before starting the wave
        yield return new WaitForSeconds(1f);
        
        // Spawn enemies with delay between each spawn
        for (int i = 0; i < remainingEnemiesInWave; i++)
        {
            if(bossPrefab && currentWave == gameManager.TotalWaves && i == remainingEnemiesInWave - 1){
                SpawnBoss();
            }else{
                SpawnEnemy();
            }
            yield return new WaitForSeconds(timeBetweenSpawns);
        }
        
        isSpawning = false;
        
        // Check if all enemies are already dead (might be rare, but possible)
        CheckWaveComplete();
    }
    
    void SpawnEnemy()
    {
        if (enemyPrefabs == null)
        {
            Debug.LogError("Enemy prefab not assigned to EnemySpawner!");
            return;
        }
        
        int index = seededRandom.Next(0, enemyPrefabs.Length);
        GameObject selectedPrefab = enemyPrefabs[index];
        // Instantiate the enemy at the spawn point
        GameObject enemyInstance = Instantiate(selectedPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Spawned enemy at {spawnPoint.position}");
        
        // Add to active enemies list
        activeEnemies.Add(enemyInstance);
        
        // Setup enemy component
        Enemy enemy = enemyInstance.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Scale difficulty with wave number
            enemy.maxHealth += (currentWave - 1) * 20f; // +20 health per wave
            enemy.currentHealth = enemy.maxHealth;
            
            // Pass a reference to this spawner so enemies can notify when they die
            enemy.spawner = this;
        }
    }

    void SpawnBoss()
    {
        if (bossPrefab == null)
        {
            Debug.LogError("Enemy prefab not assigned to EnemySpawner!");
            return;
        }
    
        // Instantiate the enemy at the spawn point
        GameObject enemyInstance = Instantiate(bossPrefab, spawnPoint.position, spawnPoint.rotation);
        Debug.Log($"Spawned enemy at {spawnPoint.position}");
        
        // Add to active enemies list
        activeEnemies.Add(enemyInstance);
        
        // Setup enemy component
        Enemy enemy = enemyInstance.GetComponent<Enemy>();
        if (enemy != null)
        {
            // Scale difficulty with wave number
            enemy.maxHealth += (currentWave - 1) * 20f; // +20 health per wave
            enemy.currentHealth = enemy.maxHealth;
            
            // Pass a reference to this spawner so enemies can notify when they die
            enemy.spawner = this;
        }
    }
    
    void OnDestroy()
    {
        // Unsubscribe from events
        if (gameManager != null)
        {
            gameManager.OnWaveChanged -= HandleWaveChanged;
        }
    }
} 