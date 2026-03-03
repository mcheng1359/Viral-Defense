using UnityEngine;

public class ArtilleryTurret : Turret
{
    [Header("Artillery Properties")]
    public float explosionRadius = 5f;
    public float minDamagePercent = 0.3f;
    private AudioSource shootSFX;
    
    private void Awake()
    {
        
        shootSFX = GetComponent<AudioSource>();
        turretName = "Artillery Turret";
        description = "Slow-firing turret with area damage.";
        fireRate = 0.5f; 
        damage = 40f;    
        cost = 250;
        range = 17f;     
        
        Debug.Log($"Artillery Turret initialized: Damage={damage}, ExplosionRadius={explosionRadius}");
    }
    
    protected void Start()
    {

        if (projectilePrefab == null)
        {
            Debug.LogError("Artillery Turret missing projectile prefab!");
            return;
        }
        
        // Check if prefab has ExplosiveProjectile component
        ExplosiveProjectile testProjectile = projectilePrefab.GetComponent<ExplosiveProjectile>();
        if (testProjectile == null)
        {
            Debug.LogError("Artillery Turret's projectile prefab is missing ExplosiveProjectile component!");
        }
        else
        {
            Debug.Log("Artillery Turret's projectile prefab has ExplosiveProjectile component");
        }
    }
    
    protected override void Shoot()
    {
        if (currentTarget == null)
        {
            Debug.LogWarning("Artillery Turret trying to shoot with null target!");
            return;
        }
        
        Debug.Log($"Artillery Turret shooting at {currentTarget.name} with damage {damage} and radius {explosionRadius}");
        
        // Instantiate the projectile at the firepoint
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        if(shootSFX){
            AudioSource.PlayClipAtPoint(shootSFX.clip, projectile.transform.position, 0.5f);
        }
        if (projectile == null)
        {
            Debug.LogError("Failed to instantiate artillery projectile!");
            return;
        }
        
        // Get the ExplosiveProjectile component
        ExplosiveProjectile ep = projectile.GetComponent<ExplosiveProjectile>();
        if (ep != null)
        {
            ep.Initialize(currentTarget, damage, explosionRadius, minDamagePercent);
            Debug.Log($"Artillery projectile initialized successfully with target: {currentTarget.name}");
        }
        else
        {
            Debug.LogError("Artillery turret projectile prefab missing ExplosiveProjectile component!");
            Destroy(projectile);  // Clean up the projectile if it's missing the component
        }
    }
    
    private void OnDrawGizmosSelected()
    {
        // Don't call the base method since it's inaccessible
        
        // Show explosion radius
        if (firePoint != null)
        {
            Gizmos.color = new Color(1f, 0.5f, 0f, 0.3f);  // Semi-transparent orange
            Gizmos.DrawSphere(firePoint.position, explosionRadius);
        }
    }
} 