using UnityEngine;

public class PlayerProjectile : MonoBehaviour {
    public float damage = 25f;
    public float speed = 20f;
    public float lifetime = 3f;

    void Start() {
        Destroy(gameObject, lifetime);
    }

    void Update() {
        transform.Translate(Vector3.forward * speed * Time.deltaTime);
    }

    void OnCollisionEnter(Collision collision) {
        if (collision.gameObject.TryGetComponent<Enemy>(out var enemy)) {
            enemy.TakeDamage(damage);
        }
        
        Destroy(gameObject);
    }
}