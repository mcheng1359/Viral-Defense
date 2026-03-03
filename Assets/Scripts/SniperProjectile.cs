using UnityEngine;

public class SniperProjectile : MonoBehaviour
{
    public float speed = 50f; // Very fast projectile
    public GameObject hitEffectPrefab; // Regular hit effect
    public GameObject criticalHitEffectPrefab; // Special effect for critical hits
    public TrailRenderer bulletTrail; // Optional trail renderer
    
    private Enemy target;
    private float damage;
    private bool isCritical;
    
    public void Initialize(Enemy target, float damage, bool isCritical)
    {
        this.target = target;
        this.damage = damage;
        this.isCritical = isCritical;
        
        // Apply a 90-degree rotation on X axis to fix orientation
        transform.Rotate(0, 0, 0);
        
        // Set trail color based on critical hit (optional)
        if (bulletTrail != null && isCritical)
        {
            // Set trail color to something more dramatic for critical hits
            bulletTrail.startColor = Color.red;
            bulletTrail.endColor = Color.yellow;
        }
    }
    
    void Update()
    {
        if (target == null)
        {
            Destroy(gameObject);
            return;
        }
        
        // Move toward target
        Vector3 dir = target.transform.position - transform.position;
        float distanceThisFrame = speed * Time.deltaTime;
        
        if (dir.magnitude <= distanceThisFrame)
        {
            HitTarget();
            return;
        }
        
        transform.Translate(dir.normalized * distanceThisFrame, Space.World);
        transform.LookAt(target.transform);
    }
    
    void HitTarget()
    {
        if (target != null)
        {
            // Apply damage to enemy
            target.TakeDamage(damage);
            
            // Show hit effect
            Vector3 hitPosition = transform.position;
            
            // Show critical hit effect if needed
            if (isCritical && criticalHitEffectPrefab != null)
            {
                GameObject effect = Instantiate(criticalHitEffectPrefab, hitPosition, Quaternion.identity);
                
                // Auto-destroy the effect
                ParticleSystem ps = effect.GetComponent<ParticleSystem>();
                if (ps != null)
                {
                    float duration = ps.main.duration + ps.main.startLifetime.constant;
                    Destroy(effect, duration);
                }
                else
                {
                    Destroy(effect, 2f);
                }
                
                Debug.Log($"Critical hit! Dealt {damage} damage to {target.name}");
            }
            else if (hitEffectPrefab != null)
            {
                // Regular hit effect
                GameObject effect = Instantiate(hitEffectPrefab, hitPosition, Quaternion.identity);
                Destroy(effect, 2f);
                
                Debug.Log($"Hit! Dealt {damage} damage to {target.name}");
            }
        }
        
        Destroy(gameObject);
    }
} 