using UnityEngine;

public class CameraToggle : MonoBehaviour
{
    [Header("Cameras")]
    public Camera playerCamera;      // First-person camera
    public Camera topDownCamera;    // Top-down camera for building
    public int offset = 18;
    
    [Header("Controls")]
    public KeyCode toggleKey = KeyCode.E;
    
    [Header("Player Components")]
    public PlayerMovement playerMovement;
    public PlayerShooting playerShooting;
    public MouseLook mouseLook;
    
    [Header("Building")]
    public LayerMask buildableTileLayer;
    public TurretManager turretManager;
    public Canvas buildUI;           // UI specific to build mode (turret buttons only)
    
    [Header("UI Elements")]
    public GameObject crosshair;     // Reference to just the crosshair element
    
    private bool isInTopDownView = false;
    private BuildableTile hoveredTile;
    private bool wasMouseLockedBeforeTopDown;
    
    void Start()
    {
        // Find components if not assigned
        if (playerMovement == null) 
        {
            playerMovement = GetComponent<PlayerMovement>();
            Debug.Log("CameraToggle: PlayerMovement " + (playerMovement != null ? "found" : "NOT FOUND"));
        }
        
        if (playerShooting == null) 
        {
            playerShooting = GetComponent<PlayerShooting>();
            Debug.Log("CameraToggle: PlayerShooting " + (playerShooting != null ? "found" : "NOT FOUND"));
        }
        
        // Fix for MouseLook being on a child GameObject
        if (mouseLook == null)
        {
            // Try to find on camera (if assigned)
            if (playerCamera != null)
            {
                // Try direct GetComponent
                mouseLook = playerCamera.GetComponent<MouseLook>();
                
                // If not found, search in playerCamera's children
                if (mouseLook == null)
                {
                    mouseLook = playerCamera.GetComponentInChildren<MouseLook>();
                }
                
                // If still not found, check if the camera has a parent with MouseLook
                if (mouseLook == null && playerCamera.transform.parent != null)
                {
                    mouseLook = playerCamera.transform.parent.GetComponent<MouseLook>();
                }
            }
            
            // If still not found, search in all children of this GameObject
            if (mouseLook == null)
            {
                mouseLook = GetComponentInChildren<MouseLook>();
            }
            
            // Last resort: find any MouseLook in the scene
            if (mouseLook == null)
            {
                mouseLook = FindObjectOfType<MouseLook>();
            }
            
            Debug.Log("CameraToggle: MouseLook " + (mouseLook != null ? "found" : "NOT FOUND"));
            if (mouseLook != null)
            {
                Debug.Log("CameraToggle: MouseLook found on GameObject: " + mouseLook.gameObject.name);
            }
        }
        
        if (turretManager == null) 
        {
            turretManager = FindObjectOfType<TurretManager>();
            Debug.Log("CameraToggle: TurretManager " + (turretManager != null ? "found" : "NOT FOUND"));
        }
        
        // Validate cameras
        if (playerCamera == null)
            Debug.LogError("CameraToggle: Player camera is missing!");
            
        if (topDownCamera == null)
            Debug.LogError("CameraToggle: Top-down camera is missing!");
            
        // Validate UI
        if (buildUI == null)
            Debug.LogWarning("CameraToggle: Build UI canvas is not assigned!");
            
        // Log buildable layer mask
        Debug.Log("CameraToggle: Buildable layer mask value: " + buildableTileLayer.value);
        
        // Make sure top-down camera is disabled at start
        if (topDownCamera != null)
            topDownCamera.gameObject.SetActive(false);
            
        // Make sure player camera is enabled
        if (playerCamera != null)
            playerCamera.gameObject.SetActive(true);
            
        // Hide build UI at start
        if (buildUI != null)
            buildUI.gameObject.SetActive(false);
            
        // Log summary of all references
        Debug.Log("CameraToggle initialization complete. Ready to toggle with " + toggleKey);
    }
    
    void Update()
    {
        if (Input.GetKeyDown(toggleKey))
        {
            if(topDownCamera){
                topDownCamera.transform.position = new Vector3(playerCamera.transform.position.x, offset, playerCamera.transform.position.z);
            }
            ToggleCamera();
        }
        
        // Only handle building in top-down view
        if (isInTopDownView)
        {
            HandleTileSelection();
        }
    }
    
    void ToggleCamera()
    {
        isInTopDownView = !isInTopDownView;
        
        if (isInTopDownView)
        {
            // Store current mouse lock state before switching
            wasMouseLockedBeforeTopDown = (Cursor.lockState == CursorLockMode.Locked);
            
            // Switch to top-down view
            if (topDownCamera != null) topDownCamera.gameObject.SetActive(true);
            if (playerCamera != null) playerCamera.gameObject.SetActive(false);
            
            // Disable player movement controls
            if (playerMovement != null) playerMovement.enabled = false;
            if (playerShooting != null) playerShooting.enabled = false;
            
            // Show cursor
            Cursor.visible = true;
            Cursor.lockState = CursorLockMode.None;
            
            // Show build UI
            if (buildUI != null) buildUI.gameObject.SetActive(true);
            
            // Hide crosshair
            if (crosshair != null) crosshair.SetActive(false);
        }
        else
        {
            // Switch back to first-person view
            if (topDownCamera != null) topDownCamera.gameObject.SetActive(false);
            if (playerCamera != null) playerCamera.gameObject.SetActive(true);
            
            // Re-enable player movement controls
            if (playerMovement != null) playerMovement.enabled = true;
            if (playerShooting != null) playerShooting.enabled = true;
            
            // Restore previous mouse lock state instead of forcing it locked
            if (wasMouseLockedBeforeTopDown)
            {
                Cursor.visible = false;
                Cursor.lockState = CursorLockMode.Locked;
            }
            else
            {
                Cursor.visible = true;
                Cursor.lockState = CursorLockMode.None;
            }
            
            // Hide build UI
            if (buildUI != null) buildUI.gameObject.SetActive(false);
            
            // Show crosshair
            if (crosshair != null) crosshair.SetActive(true);
            
            // Clear any selection
            if (turretManager != null)
                turretManager.CancelPlacement();
                
            // Clear any hover highlight
            if (hoveredTile != null)
            {
                hoveredTile.ResetHighlight();
                hoveredTile = null;
            }
        }
    }
    
    void HandleTileSelection()
    {
        // Reset previous hover if any
        if (hoveredTile != null)
        {
            hoveredTile.ResetHighlight();
            hoveredTile = null;
        }
        
        // Cast ray from mouse position
        Ray ray = topDownCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        if (Physics.Raycast(ray, out hit, 100f, buildableTileLayer))
        {
            BuildableTile tile = hit.collider.GetComponent<BuildableTile>();
            
            if (tile != null)
            {
                // Highlight the tile
                hoveredTile = tile;
                hoveredTile.Highlight();
                
                // Select tile on left click
                if (Input.GetMouseButtonDown(0) && turretManager != null)
                {
                    turretManager.SelectTile(tile);
                }
                
                // Sell turret on right click if one exists
                if (Input.GetMouseButtonDown(1) && turretManager != null)
                {
                    if (tile.HasTurret)
                    {
                        turretManager.SellTurret(tile);
                    }
                    else
                    {
                        turretManager.CancelPlacement();
                    }
                }
            }
        }
        else
        {
            // Right click to cancel when not over a tile
            if (Input.GetMouseButtonDown(1) && turretManager != null)
            {
                turretManager.CancelPlacement();
            }
        }
    }
    
    public bool IsInTopDownView()
    {
        return isInTopDownView;
    }
} 