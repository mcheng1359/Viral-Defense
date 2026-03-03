using UnityEngine;

public class SniperTurret : Turret
{
    [Header("Sniper Properties")]
    public float critChance = 0.25f;      // Chance to deal critical hit
    public float critDamageMultiplier = 2.5f;  // Damage multiplier on critical hit
    public LineRenderer laserSight;
    private AudioSource shootSFX;
    
    private void Awake()
    {
        // Set default values for sniper
        shootSFX = GetComponent<AudioSource>();
        turretName = "Sniper Turret";
        description = "Long-range turret with high damage but slow fire rate.";
        fireRate = 0.25f; // Very slow fire rate (shots per second)
        damage = 80f;     // High damage
        cost = 300;
        range = 125f;      // Long range
    }
    
    protected override void Update()
    {
        base.Update();
        
        // Update laser sight if available
        if (laserSight != null)
        {
            if (currentTarget != null)
            {
                laserSight.enabled = true;
                laserSight.SetPosition(0, firePoint.position);
                laserSight.SetPosition(1, currentTarget.transform.position);
            }
            else
            {
                laserSight.enabled = false;
            }
        }
    }
    
    protected override void Shoot()
    {
        // Determine if this shot is a critical hit
        bool isCritical = Random.value <= critChance;
        float actualDamage = damage;
        
        if (isCritical)
        {
            actualDamage *= critDamageMultiplier;
            Debug.Log($"Critical shot fired! Damage increased to {actualDamage}");
        }
        
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        SniperProjectile sp = projectile.GetComponent<SniperProjectile>();
        if (sp != null)
        {
            if(shootSFX){
            AudioSource.PlayClipAtPoint(shootSFX.clip, projectile.transform.position, 0.5f);
            }
            sp.Initialize(currentTarget, actualDamage, isCritical);
        }
        else
        {
            Debug.LogError("Sniper turret projectile prefab missing SniperProjectile component!");
        }
    }
} 