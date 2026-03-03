using UnityEngine;

public class WaypointPath : MonoBehaviour
{
    public Transform[] waypoints;
    
    void Start()
    {
        // Auto-initialize waypoints from children if not set
        if (waypoints == null || waypoints.Length == 0)
        {
            SetupWaypointsFromChildren();
            Debug.Log($"WaypointPath auto-initialized with {waypoints.Length} waypoints from children");
        }
    }
    
    void OnDrawGizmos()
    {
        if (waypoints == null || waypoints.Length < 2)
            return;
        
        // Draw lines between waypoints
        Gizmos.color = Color.cyan;
        for (int i = 0; i < waypoints.Length - 1; i++)
        {
            if (waypoints[i] != null && waypoints[i+1] != null)
                Gizmos.DrawLine(waypoints[i].position, waypoints[i+1].position);
        }
        
        // Draw spheres at waypoint positions
        Gizmos.color = Color.blue;
        foreach (Transform waypoint in waypoints)
        {
            if (waypoint != null)
                Gizmos.DrawWireSphere(waypoint.position, 0.5f);
        }
    }
    
    // Helper method to get all child objects as waypoints
    public void SetupWaypointsFromChildren()
    {
        waypoints = new Transform[transform.childCount];
        for (int i = 0; i < transform.childCount; i++)
        {
            waypoints[i] = transform.GetChild(i);
        }
    }
    
    // Editor menu item to quickly set up waypoints
    [ContextMenu("Setup Waypoints From Children")]
    public void EditorSetupWaypoints()
    {
        SetupWaypointsFromChildren();
        Debug.Log($"Waypoints initialized with {waypoints.Length} points");
    }
} 