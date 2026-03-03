using UnityEngine;

public class GunController : MonoBehaviour
{
    public GameObject bulletPrefab;  //bullet to spawn
    public Transform firePoint;  //position to spawn bullets
    public float fireForce = 500f; //bullet launch force
    public float fireRate = 0.2f; //time between shots
    private float nextFireTime = 0f;
    public int maxAmmo = 10;
    private int currentAmmo;

    void Shoot() {
        if (bulletPrefab == null) {
            Debug.LogError("Bullet prefab is missing! Assign it in the Inspector.");
            return; //prevent shooting if prefab is missing
        }

        currentAmmo--;

        //spawn bullet at fire point
        GameObject bullet = Instantiate(bulletPrefab, firePoint.position, firePoint.rotation);
    
        if (bullet.TryGetComponent<Rigidbody>(out Rigidbody rb)) {
            rb.AddForce(firePoint.forward * fireForce, ForceMode.Impulse);
        } else {
            Debug.LogError("Bullet prefab is missing a Rigidbody!");
        }
        Debug.Log("Bang! Ammo left: " + currentAmmo);
    }

    void Update() {
        if (Input.GetButtonDown("Fire1") && Time.time >= nextFireTime && currentAmmo > 0) {
            Shoot();
            nextFireTime = Time.time + fireRate;
        }

        if (Input.GetKeyDown(KeyCode.R)) {
            Reload();
        }
    }

    void Reload() {
        Debug.Log("Reloading...");
        currentAmmo = maxAmmo;
    }
}