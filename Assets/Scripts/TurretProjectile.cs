using UnityEngine;

public class TurretProjectile : MonoBehaviour
{
    private Enemy target;
    private float damage;
    private float speed = 30f;
    
    public void Initialize(Enemy target, float damage)
    {
        this.target = target;
        this.damage = damage;
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
        }
        
        Destroy(gameObject);
    }
}