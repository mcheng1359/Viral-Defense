using UnityEngine;

public abstract class Turret : MonoBehaviour
{
    [Header("Base Turret Stats")]
    public float range = 10f;
    public float fireRate = 1f;
    public float damage = 20f;
    public int cost = 100;
    public GameObject projectilePrefab;
    public Transform firePoint;
    public Transform turretHead;  
    
    [Header("Idle Scanning")]
    public float scanningSpeed = 45f;      // Degrees per second
    public float scanningAngle = 90f;      // Total angle to scan
    
    [Header("Turret Info")]
    public string turretName = "Basic Turret";
    public string description = "A basic turret that fires at enemies.";
    public Sprite icon;
    
    private float nextFireTime;
    protected Enemy currentTarget;
    private float currentScanAngle;
    private int scanDirection = 1;         // 1 is clockwise, -1 is counter-clockwise
    
    protected virtual void Update()
    {
        if (currentTarget == null || !IsInRange(currentTarget.transform.position))
        {
            // Target is either null or out of range, find a new one
            currentTarget = null; // Clear the current target to stop tracking
            FindNewTarget();
        }
        
        if (currentTarget != null)
        {
            // Target tracking mode - look at the target in full 3D space
            Vector3 targetPosition = currentTarget.transform.position;
            // No Y restriction - allow looking up and down
            turretHead.LookAt(targetPosition);
            
            // Double check target is in range before firing
            if (IsInRange(targetPosition))
            {
                // Fire if cooldown is over
                if (Time.time >= nextFireTime)
                {
                    Shoot();
                    nextFireTime = Time.time + 1f / fireRate; // Convert to cooldown time
                }
            }
        }
        else
        {
            // Idle scanning mode when no target is available
            IdleScanning();
        }
    }
    
    void IdleScanning()
    {
        // Update the current scan angle
        currentScanAngle += scanDirection * scanningSpeed * Time.deltaTime;
        
        // Reverse direction when reaching scan limits
        if (Mathf.Abs(currentScanAngle) >= scanningAngle / 2)
        {
            scanDirection *= -1;
            currentScanAngle = Mathf.Sign(currentScanAngle) * scanningAngle / 2;
        }
        
        // Calculate a point to look at based on the scan angle
        Vector3 scanDirVector = transform.forward;
        scanDirVector = Quaternion.Euler(0, currentScanAngle, 0) * scanDirVector;
        
        // Create a point in that direction to look at
        Vector3 targetPoint = turretHead.position + scanDirVector * 10f;
        // Allow some slight up/down variation during scanning (optional)
        // targetPoint.y += Mathf.Sin(Time.time * 0.5f) * 2f; // Uncomment for slight up/down motion
        
        // Look at the calculated point
        turretHead.LookAt(targetPoint);
    }
    
    protected virtual void FindNewTarget()
    {
        Enemy[] enemies = GameObject.FindObjectsOfType<Enemy>();
        float nearestDistance = float.MaxValue;
        
        foreach (Enemy enemy in enemies)
        {
            float distance = Vector3.Distance(transform.position, enemy.transform.position);
            if (distance <= range && distance < nearestDistance)
            {
                currentTarget = enemy;
                nearestDistance = distance;
            }
        }
    }
    
    protected bool IsInRange(Vector3 targetPosition)
    {
        return Vector3.Distance(transform.position, targetPosition) <= range;
    }
    
    protected virtual void Shoot()
    {
        // Create the projectile
        GameObject projectile = Instantiate(projectilePrefab, firePoint.position, firePoint.rotation);
        
        // Try to use the target-tracking TurretProjectile component
        TurretProjectile tp = projectile.GetComponent<TurretProjectile>();
        if (tp != null && currentTarget != null)
        {
            tp.Initialize(currentTarget, damage);
            Debug.Log("Fired target-tracking projectile");
        }
        else
        {
            // No TurretProjectile component, apply direct physics motion
            Rigidbody rb = projectile.GetComponent<Rigidbody>();
            if (rb != null)
            {
                // Adjust this force as needed for your game
                rb.useGravity = false; // Typically bullets don't use gravity
                rb.linearVelocity = Vector3.zero; // Reset any initial velocity
                
                // Apply velocity in the direction the firePoint is facing
                Vector3 shootDirection = firePoint.forward;
                float projectileSpeed = 30f; // Adjust this speed as needed
                rb.linearVelocity = shootDirection * projectileSpeed;
                
                Debug.Log($"Applied physics velocity to projectile: {rb.linearVelocity}");
                
                // Set up auto-destruction to avoid cluttering the scene
                Destroy(projectile, 5f);
            }
            else
            {
                Debug.LogWarning("Projectile has no movement script or Rigidbody component!");
            }
        }
    }
    
    void OnDrawGizmosSelected() {
        Gizmos.color = Color.red;
        Gizmos.DrawWireSphere(transform.position, range);
        
        // Visualize scanning angle in editor
        Gizmos.color = Color.yellow;
        Vector3 rightBoundary = Quaternion.Euler(0, scanningAngle / 2, 0) * transform.forward;
        Vector3 leftBoundary = Quaternion.Euler(0, -scanningAngle / 2, 0) * transform.forward;
        Gizmos.DrawLine(transform.position, transform.position + rightBoundary * range);
        Gizmos.DrawLine(transform.position, transform.position + leftBoundary * range);
    }
}
