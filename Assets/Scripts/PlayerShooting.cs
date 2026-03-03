using UnityEngine;

public class PlayerShooting : MonoBehaviour
{
    public GameObject projectilePrefab;
    public Transform shootPoint;
    public float shootCooldown = 0.5f;
    public float projectileForce = 1000f;
    
    private float nextShootTime;
    private Camera playerCamera;
    private bool debugLogging = true; // Set to false to disable debug logs
    private AudioSource shootSFX;

    void Start()
    {
        shootSFX = GetComponent<AudioSource>();
        playerCamera = GetComponentInChildren<Camera>();
        
        if (shootPoint == null)
        {
            GameObject newShootPoint = new GameObject("ShootPoint");
            shootPoint = newShootPoint.transform;
            shootPoint.SetParent(playerCamera.transform);
            shootPoint.localPosition = new Vector3(0, 0, 0.5f);
        }
        
        if (debugLogging)
            Debug.Log("PlayerShooting initialized. Using camera: " + playerCamera.name);
    }

    void Update()
    {
        if (Input.GetMouseButtonDown(0) && Time.time >= nextShootTime)
        {
            if (debugLogging)
                Debug.Log("PlayerShooting detected mouse click - shooting");
                
            Shoot();
            nextShootTime = Time.time + shootCooldown;
        }
    }

    void Shoot()
    {
        if(!GameUI.isPlaying){
            return;
        }
        //create the projectile at the shoot point
        GameObject projectile = Instantiate(projectilePrefab, shootPoint.position, playerCamera.transform.rotation);

        if(shootSFX){
            AudioSource.PlayClipAtPoint(shootSFX.clip, projectile.transform.position, 0.2f);
        }
        
        Rigidbody rb = projectile.GetComponent<Rigidbody>();
        if (rb != null)
        {
            //zero out any existing velocity
            rb.linearVelocity = Vector3.zero;
            rb.angularVelocity = Vector3.zero;
            
            //add force in the direction you're looking
            rb.AddForce(playerCamera.transform.forward * projectileForce);
        }
    }
    
    public void OnEnable()
    {
        if (debugLogging)
            Debug.Log("PlayerShooting ENABLED");
    }
    
    public void OnDisable()
    {
        if (debugLogging)
            Debug.Log("PlayerShooting DISABLED");
    }
}