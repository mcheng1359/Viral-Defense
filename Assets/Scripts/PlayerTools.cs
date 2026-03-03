using UnityEngine;

public enum ToolType
{
    Gun,
    BuildHammer
}

public class PlayerTools : MonoBehaviour
{
    [Header("Tool References")]
    public GameObject gunObject;
    public GameObject hammerObject;
    
    [Header("Building")]
    public LayerMask buildableTileLayer;
    public float buildRange = 5f;
    public KeyCode buildModeInteractionKey = KeyCode.E; // Key to temporarily enable cursor for interaction
    
    private ToolType currentTool = ToolType.Gun;
    private PlayerShooting shootingScript;
    private TurretManager turretManager;
    private Camera playerCamera;
    private BuildableTile lastHoveredTile;
    private BuildableTile selectedTile; // Track the currently selected tile
    private bool interactionMode = false;
    
    // Public method to check if player is in build mode
    public bool IsInBuildMode()
    {
        return currentTool == ToolType.BuildHammer;
    }
    
    // Public method to check if we're in interaction mode (cursor unlocked in build mode)
    public bool IsInInteractionMode()
    {
        return currentTool == ToolType.BuildHammer && interactionMode;
    }
    
    void Start()
    {
        shootingScript = GetComponent<PlayerShooting>();
        turretManager = FindObjectOfType<TurretManager>();
        playerCamera = GetComponentInChildren<Camera>();
        
        // Debug layer mask and camera
        Debug.Log("BuildableTileLayer mask value: " + buildableTileLayer.value);
        Debug.Log("Using camera: " + (playerCamera != null ? playerCamera.name : "none found"));
        Debug.Log("TurretManager found: " + (turretManager != null));
        
        // Initial tool setup
        SwitchTool(ToolType.Gun);
    }
    
    void Update()
    {
        // Tool switching with scroll wheel or number keys
        if (Input.GetAxis("Mouse ScrollWheel") > 0 || Input.GetKeyDown(KeyCode.Alpha1))
        {
            SwitchTool(ToolType.Gun);
        }
        else if (Input.GetAxis("Mouse ScrollWheel") < 0 || Input.GetKeyDown(KeyCode.Alpha2))
        {
            SwitchTool(ToolType.BuildHammer);
        }
        
        // Toggle interaction mode in build mode
        if (currentTool == ToolType.BuildHammer && Input.GetKeyDown(buildModeInteractionKey))
        {
            ToggleInteractionMode();
        }
        
        // Tool-specific functionality
        if (currentTool == ToolType.BuildHammer)
        {
            UpdateBuildMode();
            
            // Click to select a tile for building (in build mode, regardless of interaction mode)
            if (Input.GetMouseButtonDown(0))
            {
                Debug.Log("Mouse button clicked in build mode!");
                SelectTileForBuilding();
            }
            
            // Right-click or escape to cancel
            if (Input.GetMouseButtonDown(1) || Input.GetKeyDown(KeyCode.Escape))
            {
                Debug.Log("Cancellation attempted");
                if (turretManager != null)
                {
                    turretManager.CancelPlacement();
                }
                
                // Exit interaction mode on cancel
                if (interactionMode)
                {
                    ExitInteractionMode();
                }
            }
        }
        else if (lastHoveredTile != null)
        {
            // Clear highlighting when not in build mode
            lastHoveredTile.ResetHighlight();
            lastHoveredTile = null;
        }
    }
    
    void SwitchTool(ToolType newTool)
    {
        // Cancel any ongoing placement when switching tools
        if (currentTool == ToolType.BuildHammer && turretManager != null)
        {
            turretManager.CancelPlacement();
        }
        
        // Exit interaction mode if we're in it
        if (interactionMode)
        {
            ExitInteractionMode();
        }
        
        // Deselect any selected tile
        DeselectCurrentTile();
        
        // Update the current tool
        currentTool = newTool;
        
        // Enable/disable relevant game objects
        if (gunObject != null) 
            gunObject.SetActive(currentTool == ToolType.Gun);
        
        if (hammerObject != null)
            hammerObject.SetActive(currentTool == ToolType.BuildHammer);
        
        // Enable/disable shooting functionality
        if (shootingScript != null)
            shootingScript.enabled = (currentTool == ToolType.Gun);
        
        Debug.Log("Switched to " + currentTool.ToString());
    }
    
    void ToggleInteractionMode()
    {
        if (interactionMode)
        {
            ExitInteractionMode();
        }
        else
        {
            EnterInteractionMode();
        }
    }
    
    void EnterInteractionMode()
    {
        interactionMode = true;
        MouseLook mouseLook = Camera.main?.GetComponent<MouseLook>();
        if (mouseLook != null)
        {
            mouseLook.UnlockCursor();
        }
        Debug.Log("Entered interaction mode - cursor unlocked");
    }
    
    void ExitInteractionMode()
    {
        interactionMode = false;
        MouseLook mouseLook = Camera.main?.GetComponent<MouseLook>();
        if (mouseLook != null)
        {
            mouseLook.LockCursor();
        }
        Debug.Log("Exited interaction mode - cursor locked");
    }
    
    void UpdateBuildMode()
    {
        // Reset last hovered tile if we had one
        if (lastHoveredTile != null)
        {
            lastHoveredTile.ResetHighlight();
            lastHoveredTile = null;
        }
        
        // Raycast to find buildable tiles
        Ray ray = playerCamera.ScreenPointToRay(Input.mousePosition);
        RaycastHit hit;
        
        // Debug raycast
        Debug.DrawRay(ray.origin, ray.direction * buildRange, Color.yellow);
        
        if (Physics.Raycast(ray, out hit, buildRange, buildableTileLayer))
        {
            Debug.Log("Hit something on layer: " + LayerMask.LayerToName(hit.collider.gameObject.layer));
            
            BuildableTile tile = hit.collider.GetComponent<BuildableTile>();
            
            if (tile != null)
            {
                Debug.Log("Found BuildableTile: " + tile.name);
                
                // Store reference to this tile and highlight it
                lastHoveredTile = tile;
                lastHoveredTile.Highlight();
            }
            else
            {
                Debug.Log("Hit object doesn't have BuildableTile component: " + hit.collider.gameObject.name);
            }
        }
    }
    
    void SelectTileForBuilding()
    {
        Debug.Log("SelectTileForBuilding called. lastHoveredTile: " + (lastHoveredTile != null ? lastHoveredTile.name : "null"));
        
        // Deselect current tile if we're clicking on a different one or the same one again
        if (selectedTile != null)
        {
            // If we're clicking the same tile, just deselect it
            if (selectedTile == lastHoveredTile)
            {
                DeselectCurrentTile();
                return;
            }
            else
            {
                // Deselect the old tile before selecting the new one
                DeselectCurrentTile();
            }
        }
        
        if (lastHoveredTile != null)
        {
            Debug.Log("HasTurret: " + lastHoveredTile.HasTurret + ", TurretManager: " + (turretManager != null ? "found" : "null"));
            
            // Store reference to selected tile
            selectedTile = lastHoveredTile;
            
            // Directly call OnTileSelected to change the tile color
            selectedTile.OnTileSelected();
            
            // Also tell TurretManager about the selected tile (if needed)
            if (turretManager != null)
            {
                turretManager.SelectTile(selectedTile);
            }
        }
        else
        {
            Debug.Log("No valid tile to select");
        }
    }
    
    // Deselect the currently selected tile
    public void DeselectCurrentTile()
    {
        if (selectedTile != null)
        {
            // Tell the tile it's deselected
            selectedTile.Deselect();
            
            // Also tell TurretManager (if needed)
            if (turretManager != null)
            {
                turretManager.CancelPlacement();
            }
            
            selectedTile = null;
        }
    }
    
    // For testing - you can call this from UI buttons if needed
    public void TestSelectCurrentTile()
    {
        Debug.Log("TestSelectCurrentTile called");
        if (lastHoveredTile != null && turretManager != null)
        {
            Debug.Log("Forcing tile selection: " + lastHoveredTile.name);
            turretManager.SelectTile(lastHoveredTile);
        }
        else
        {
            Debug.Log("Cannot force select - no tile or no manager");
        }
    }
} 