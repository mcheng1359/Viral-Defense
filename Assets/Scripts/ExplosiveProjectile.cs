using UnityEngine;

public class ExplosiveProjectile : MonoBehaviour
{
    public float speed = 20f;
    public GameObject explosionEffectPrefab; // Assign a particle effect in the inspector
    public LayerMask enemyLayerMask; // Set this to the enemy layer in inspector
    
    private Enemy target;
    private float damage;
    private float explosionRadius;
    private float minDamagePercent;
    
    public void Initialize(Enemy target, float damage, float explosionRadius, float minDamagePercent)
    {
        this.target = target;
        this.damage = damage;
        this.explosionRadius = explosionRadius;
        this.minDamagePercent = minDamagePercent;
        
        Debug.Log($"ExplosiveProjectile initialized: Target={target.name}, Damage={damage}, Radius={explosionRadius}");
    }
    
    void Start()
    {
        // Log creation
        Debug.Log($"ExplosiveProjectile created at {transform.position} targeting {(target != null ? target.name : "null")}");
    }
    
    void Update()
    {
        if (target == null)
        {
            Debug.Log("Target is null, exploding...");
            Explode(); // Target destroyed, explode anyway
            return;
        }
        
        // Move toward target
        Vector3 dir = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        
        if (dir.magnitude <= distanceThisFrame)
        {
            Debug.Log($"Reached target at {target.transform.position}, exploding...");
            Explode();
            return;
        }
        
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target.transform);
    }
    
    void Explode()
    {
        Debug.Log($"Explosion at {transform.position} with radius {explosionRadius}");
        
        // Visual effect for explosion
        if (explosionEffectPrefab != null)
        {
            GameObject explosionEffect = Instantiate(explosionEffectPrefab, transform.position, Quaternion.identity);
            
            // Auto-destroy the explosion effect after a few seconds
            ParticleSystem ps = explosionEffect.GetComponent<ParticleSystem>();
            if (ps != null)
            {
                float duration = ps.main.duration + ps.main.startLifetime.constant;
                Destroy(explosionEffect, duration);
            }
            else
            {
                // If no particle system, destroy after a default time
                Destroy(explosionEffect, 2f);
            }
        }
        
        // Find all enemies in explosion radius
        Collider[] colliders = Physics.OverlapSphere(transform.position, explosionRadius);
        Debug.Log($"Found {colliders.Length} colliders in explosion radius");
        
        // If no colliders were found, try using a different approach
        if (colliders.Length == 0)
        {
            Debug.LogWarning("No colliders found in explosion radius! Trying RaycastAll as fallback...");
            // Try raycasting in all directions as a fallback
            RaycastHit[] hits = Physics.SphereCastAll(transform.position, explosionRadius, Vector3.up, 0.1f);
            Debug.Log($"SphereCastAll found {hits.Length} objects");
            
            foreach (RaycastHit hit in hits)
            {
                if (hit.collider.CompareTag("Enemy"))
                {
                    Debug.Log($"Found enemy via SphereCast: {hit.collider.name}");
                    Enemy enemy = hit.collider.GetComponentInParent<Enemy>();
                    if (enemy != null)
                    {
                        float damage = this.damage; // Full damage for fallback method
                        enemy.TakeDamage(damage);
                        Debug.Log($"Applied {damage} damage to {enemy.name} via fallback method");
                    }
                }
            }
        }
        
        // Process regular colliders
        foreach (Collider collider in colliders)
        {
            Debug.Log($"Checking collider: {collider.name}, Tag: {collider.tag}");
            
            // Try getting enemy component directly
            Enemy enemy = collider.GetComponent<Enemy>();
            
            // If not found, try parent objects
            if (enemy == null)
            {
                enemy = collider.GetComponentInParent<Enemy>();
                Debug.Log($"Tried parent lookup: {(enemy != null ? "Found" : "Not found")}");
            }
            
            if (enemy != null)
            {
                // Calculate damage based on distance (linear falloff)
                float distance = Vector3.Distance(transform.position, enemy.transform.position);
                float damagePercent = Mathf.Lerp(1f, minDamagePercent, distance / explosionRadius);
                float damageAmount = damage * damagePercent;
                
                // Apply damage to enemy
                Debug.Log($"Applying {damageAmount:F1} damage to {enemy.name} at distance {distance:F1}");
                enemy.TakeDamage(damageAmount);
            }
            else if (collider.CompareTag("Enemy"))
            {
                Debug.LogWarning($"Collider tagged as Enemy but couldn't find Enemy component: {collider.name}");
            }
        }
        
        Destroy(gameObject);
    }
    
    void OnDrawGizmosSelected()
    {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, explosionRadius);
    }

    // Add this method to handle direct collisions
    void OnCollisionEnter(Collision collision)
    {
        Debug.Log($"ExplosiveProjectile direct collision with: {collision.gameObject.name}, tag: {collision.gameObject.tag}");
        
        // If we directly hit an enemy, explode
        if (collision.gameObject.CompareTag("Enemy"))
        {
            Debug.Log("Direct hit on enemy, exploding...");
            Explode();
        }
    }

    // Add this method to handle trigger collisions
    void OnTriggerEnter(Collider other)
    {
        Debug.Log($"ExplosiveProjectile trigger entered: {other.gameObject.name}, tag: {other.gameObject.tag}");
        
        // If we trigger with an enemy, explode
        if (other.CompareTag("Enemy"))
        {
            Debug.Log("Trigger hit on enemy, exploding...");
            Explode();
        }
    }
} 