using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class Enemy : MonoBehaviour
{
    [Header("Enemy Stats")]
    public float maxHealth = 100f;
    public float moveSpeed = 3f;
    public int currencyReward = 25;
    public bool isBoss = false;
    
    [Header("UI")]
    public Slider healthBar;
    public Vector3 healthBarOffset = new(0, 1.5f, 0);
    
    [Header("Effects")]
    public GameObject deathEffect;
    
    // Reference to the spawner that created this enemy
    [HideInInspector] public EnemySpawner spawner;
    
    public float currentHealth;
    private Transform healthBarFill;
    
    // Path following variables
    private WaypointPath waypointPath; // Reference to the WaypointPath component
    private int currentWaypointIndex = 0;
    
    private TurretManager turretManager;
    private GameManager gameManager;
    
    void Start()
    {
        currentHealth = maxHealth;
        turretManager = FindObjectOfType<TurretManager>();
        gameManager = GameManager.Instance;
        
        // Find the waypoint path in the scene
        waypointPath = GameObject.FindObjectOfType<WaypointPath>();
        if (waypointPath == null)
        {
            Debug.LogError("No WaypointPath found in scene! Enemy won't move.");
        }
        else
        {
            Debug.Log($"Found waypoint path with {waypointPath.waypoints.Length} waypoints");
        }
        
        // Setup health bar
        if (healthBar != null)
        {
            //GameObject healthBarObj = Instantiate(healthBarPrefab, transform);
            //healthBarObj.transform.localPosition = healthBarOffset;

            healthBar.maxValue = maxHealth;
            healthBar.value = currentHealth;
            
            // Find the fill image
            //healthBarFill = healthBarObj.transform.Find("Fill");
            //UpdateHealthBar();
        }
    }
    
    void Update()
    {
        // If we have no waypoint path, don't try to move
        if (waypointPath == null || waypointPath.waypoints == null || waypointPath.waypoints.Length == 0)
        {
            return;
        }
        
        // If we've reached the end of the path
        if (currentWaypointIndex >= waypointPath.waypoints.Length)
        {
            ReachEnd();
            return;
        }
        
        // Get the current waypoint
        Transform targetWaypoint = waypointPath.waypoints[currentWaypointIndex];
        if (targetWaypoint == null)
        {
            Debug.LogError($"Waypoint at index {currentWaypointIndex} is null!");
            return;
        }
        
        // Move towards the waypoint
        Vector3 direction = targetWaypoint.position - transform.position;
        direction.y = 0; // Keep enemy at same height
        
        transform.Translate(direction.normalized * moveSpeed * Time.deltaTime, Space.World);
        
        // Look in movement direction
        if (direction != Vector3.zero)
        {
            transform.rotation = Quaternion.LookRotation(direction);
        }
        
        // Check if we've reached the waypoint
        float distanceSqr = (transform.position - targetWaypoint.position).sqrMagnitude;
        if (distanceSqr < 0.25f) // equivalent to distance < 0.5f
        {
            // Move to the next waypoint
            currentWaypointIndex++;
            Debug.Log($"Enemy reached waypoint {currentWaypointIndex-1}, moving to waypoint {currentWaypointIndex}");
        }
        
        // Update health bar position to face camera
        // if (healthBarFill != null && healthBarFill.parent != null)
        // {
        //     healthBarFill.parent.LookAt(Camera.main.transform);
        //     // Make sure the health bar always faces the camera (cancel X and Z rotation)
        //     Vector3 eulerAngles = healthBarFill.parent.eulerAngles;
        //     healthBarFill.parent.eulerAngles = new Vector3(0, eulerAngles.y, 0);
        // }
        if(GameObject.Find("PlayerCamera")){
            healthBar.transform.LookAt(GameObject.Find("PlayerCamera").transform);
        }else if(GameObject.Find("TopDownCamera")){
            healthBar.transform.LookAt(GameObject.Find("TopDownCamera").transform);
        }
    }
    
    void ReachEnd()
    {
        // Damage player base
        if (gameManager != null)
        {
            if(isBoss){
                gameManager.TakeDamage(500);
            }else{
                gameManager.TakeDamage(1); // Damage the base by 1
            }
            Debug.Log("Enemy reached the end! Damaged base.");
        }
        else
        {
            Debug.LogWarning("GameManager not found - can't damage base!");
        }
        
        // Notify spawner that this enemy is gone, but wasn't killed (reached end)
        if (spawner != null)
        {
            spawner.EnemyKilled(gameObject, false);
        }
        
        // Destroy the enemy
        Destroy(gameObject);
    }
    
    public void TakeDamage(float amount)
    {
        if (amount <= 0)
        {
            Debug.LogWarning($"Enemy {gameObject.name} received invalid damage amount: {amount}");
            return;
        }
        
        Debug.Log($"Enemy {gameObject.name} taking {amount} damage. Current health: {currentHealth}/{maxHealth} (ID: {GetInstanceID()})");
        
        float previousHealth = currentHealth;
        currentHealth -= amount;
        
        // Clamp health to prevent negative values
        currentHealth = Mathf.Max(0, currentHealth);
        
        float healthChange = previousHealth - currentHealth;
        float healthPercentage = currentHealth / maxHealth * 100f;
        
        Debug.Log($"Enemy health changed by {healthChange:F1} ({previousHealth:F1} → {currentHealth:F1}) - {healthPercentage:F1}% remaining (ID: {GetInstanceID()})");
        
        //UpdateHealthBar();
        if(healthBar){
            healthBar.value = currentHealth;
        }
        
        if (currentHealth <= 0)
        {
            Debug.Log($"Enemy {gameObject.name} health depleted, triggering Die() (ID: {GetInstanceID()})");
            Die();
        }
    }
    
    void Die()
    {
        // Notify spawner that this enemy is gone
        if (spawner != null)
        {
            spawner.EnemyKilled(gameObject);
        }
        // If using GameManager, don't call EnemyDefeated here - let the spawner do it
        else
        {
            // Fallback if no spawner reference, notify game manager directly
            if (gameManager != null)
            {
                gameManager.EnemyDefeated();
            }
            // If for some reason GameManager is null, fallback to direct currency reward
            else if (turretManager != null)
            {
                turretManager.AddCurrency(currencyReward);
                Debug.Log($"Warning: No GameManager or spawner found. Using TurretManager fallback for currency.");
            }
        }
        
        // Play death effect
        if (deathEffect != null)
        {
            Instantiate(deathEffect, transform.position, Quaternion.identity);
        }
        
        Debug.Log($"Enemy defeated!");
        Destroy(gameObject);
    }
    
    // void UpdateHealthBar()
    // {
    //     if (healthBarFill != null)
    //     {
    //         float healthPercent = currentHealth / maxHealth;
    //         healthBarFill.GetComponent<Image>().fillAmount = healthPercent;
    //     }
    // }
} 