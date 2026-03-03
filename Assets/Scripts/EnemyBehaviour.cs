using UnityEngine;

public class EnemyBehaviour : MonoBehaviour
{
    public float speed = 5f;
    public float health = 100f;
    public int damageToBase = 1;
    
    private WaypointPath path;
    private int currentWaypointIndex = 0;
    
    void Start() {
        path = FindFirstObjectByType<WaypointPath>();
    }
    
    void Update() {
        if (currentWaypointIndex >= path.waypoints.Length) {
            ReachedEnd();
            return;
        }
        
        //move towards next waypoint
        Vector3 targetPosition = path.waypoints[currentWaypointIndex].position;
        transform.position = Vector3.MoveTowards(
            transform.position,
            targetPosition,
            speed * Time.deltaTime
        );
        
        //look at next waypoint
        transform.LookAt(targetPosition);
        
        //check if reached waypoint
        if (Vector3.Distance(transform.position, targetPosition) < 0.1f) {
            currentWaypointIndex++;
        }
    }
    
    public void TakeDamage(float damage)
    {
        health -= damage;
        if (health <= 0)
        {
            GameManager.Instance.EnemyDefeated();
            Destroy(gameObject);
        }
    }
    
    void ReachedEnd()
    {
        GameManager.Instance.TakeDamage(damageToBase);
        Destroy(gameObject);
    }
}