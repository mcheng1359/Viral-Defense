using UnityEngine;

public class MachineGunProjectile : MonoBehaviour
{
    public float speed = 35f;
    public float damage = 15f;
    public float lifetime = 2f;  // Short lifetime for machine gun bullets
    
    private Vector3 direction;
    
    void Start()
    {
        // Set initial direction from the projectile's forward vector
        direction = transform.forward;
        
        // Destroy after lifetime to avoid filling the scene with bullets
        Destroy(gameObject, lifetime);
        
        Debug.Log($"MachineGunProjectile created at {transform.position} with damage {damage}");
    }
    
    void Update()
    {
        // Move in the set direction at constant speed
        transform.Translate(direction * speed * Time.deltaTime, Space.World);
    }
    
    // Handle collision with enemies
    private void OnTriggerEnter(Collider other)
    {
        Debug.Log($"MachineGunProjectile trigger hit: {other.gameObject.name}, tag: {other.tag}");
        
        // Try to get Enemy component from the collider's GameObject
        Enemy enemy = other.GetComponent<Enemy>();
        
        // If not found, try getting it from parent objects (common with character models)
        if (enemy == null)
        {
            enemy = other.GetComponentInParent<Enemy>();
            Debug.Log($"Searched parent for Enemy component: {(enemy != null ? "Found" : "Not found")}");
        }
        
        if (enemy != null)
        {
            // Apply damage
            Debug.Log($"Applying {damage} damage to enemy {enemy.name} via trigger");
            enemy.TakeDamage(damage);
            
            // Create hit effect (optional)
            // Instantiate(hitEffectPrefab, transform.position, Quaternion.identity);
            
            // Destroy the bullet
            Destroy(gameObject);
        }
        else
        {
            Debug.LogWarning($"Hit an object tagged 'Enemy' but couldn't find Enemy component: {other.gameObject.name}");
        }
        
        // If we hit something that isn't an enemy, destroy the bullet anyway
        // This prevents bullets from going through walls etc.
        if (!other.CompareTag("Projectile") && !other.CompareTag("Enemy"))
        {
            Debug.Log("Destroying projectile after hitting non-enemy object");
            Destroy(gameObject);
        }
    }
    
    // Alternative for collision detection if using colliders instead of triggers
    private void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"MachineGunProjectile collision with: {collision.gameObject.name}, tag: {collision.gameObject.tag}");
        
        // Try to get Enemy component from the collider's GameObject
        Enemy enemy = collision.gameObject.GetComponent<Enemy>();
        
        // If not found, try getting it from parent objects
        if (enemy == null)
        {
            enemy = collision.gameObject.GetComponentInParent<Enemy>();
            Debug.Log($"Searched parent for Enemy component: {(enemy != null ? "Found" : "Not found")}");
        }
        
        if (enemy != null)
        {
            // Apply damage
            Debug.Log($"Applying {damage} damage to enemy {enemy.name} via collision");
            enemy.TakeDamage(damage);
        }
        else if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.LogWarning($"Hit an object tagged 'Enemy' but couldn't find Enemy component: {collision.gameObject.name}");
        }
        
        // Always destroy on collision
        Destroy(gameObject);
    }
    
    // Method to set custom damage - called from the MachineGunTurret
    public void SetDamage(float newDamage)
    {
        damage = newDamage;
        Debug.Log($"MachineGunProjectile damage set to {damage}");
    }
} 