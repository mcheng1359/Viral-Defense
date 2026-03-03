using UnityEngine;

public class MachineGunTurret : Turret
{
    [Header("Machine Gun Properties")]
    public float spreadAngle = 5f; // Bullet spread in degrees
    private AudioSource shootSFX;
    
    private void Awake()
    {
        // Set default values for machine gun
        shootSFX = GetComponent<AudioSource>();
        turretName = "Machine Gun Turret";
        description = "Rapid-fire turret with moderate damage.";
        fireRate = 8f; // High fire rate
        damage = 15f;  // Lower damage per shot
        cost = 150;
        range = 20f;    // Moderate range
        
        Debug.Log($"MachineGunTurret initialized. Damage: {damage}, FireRate: {fireRate}");
    }
    
    protected override void Shoot()
    {
        Debug.Log($"MachineGunTurret shooting at {(currentTarget != null ? currentTarget.name : "no target")}");
        
        // Apply random spread for machine gun effect
        Quaternion randomSpread = Quaternion.Euler(
            Random.Range(-spreadAngle, spreadAngle),
            Random.Range(-spreadAngle, spreadAngle),
            0
        );
        
        // Create projectile with spread applied to rotation
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation * randomSpread);
        if(shootSFX){
            AudioSource.PlayClipAtPoint(shootSFX.clip, projectile.transform.position, 0.5f);
        }
        Debug.Log($"Created projectile: {projectile.name} at {firePoint.position}");
        
        // Set properties on the machine gun projectile
        MachineGunProjectile mgp = projectile.GetComponent<MachineGunProjectile>();
        if (mgp != null)
        {
            Debug.Log("Using MachineGunProjectile component");
            mgp.SetDamage(damage);
        }
        else
        {
            Debug.LogWarning("MachineGunProjectile component not found on prefab!");
            
            // Fallback to using the generic TurretProjectile
            TurretProjectile tp = projectile.GetComponent<TurretProjectile>();
            if (tp != null && currentTarget != null)
            {
                Debug.Log("Using TurretProjectile component as fallback");
                tp.Initialize(currentTarget, damage);
            }
            else
            {
                Debug.Log("No TurretProjectile component either, checking for Rigidbody");
                
                // No projectile script, so add direct physics motion
                Rigidbody rb = projectile.GetComponent<Rigidbody>();
                if (rb != null)
                {
                    Debug.Log("Using direct Rigidbody for projectile movement");
                    // Apply velocity in the direction of the spread
                    rb.useGravity = false;
                    rb.linearVelocity = projectile.transform.forward * 30f;
                    
                    // Auto-destruct
                    Destroy(projectile, 2f);
                }
                else
                {
                    Debug.LogError("MachineGun projectile has no movement components! Cannot move.");
                }
            }
        }
    }
} 