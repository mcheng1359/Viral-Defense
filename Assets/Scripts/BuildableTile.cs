using UnityEngine;
using UnityEngine.Events;

public class BuildableTile : MonoBehaviour
{
    [Header("Tile Properties")]
    public bool canBuild = true;
    
    [Header("Tile Colors")]
    public Color availableColor = new(0.5f, 1.0f, 0.5f, 0.7f);
    public Color occupiedColor = new(1.0f, 0.3f, 0.3f, 0.7f);   
    public Color highlightColor = new(1.0f, 1.0f, 0.3f, 0.8f);   
    public Color selectedColor = new(0.3f, 0.8f, 1.0f, 0.8f);   
    
    [Header("Height Indicators")]
    public float hoverHeight = 0.05f;    // How much to raise when highlighted
    public float selectionHeight = 0.1f; // How much to raise when selected
    
    [Header("Events")]
    public UnityEvent OnSelected;
    public UnityEvent<GameObject> OnTurretPlaced;
    public UnityEvent OnTurretRemoved;
    
    private Renderer tileRenderer;
    private GameObject placedTurret;
    private Material originalMaterial;
    private Color originalColor;
    private Vector3 originalPosition;
    private bool isSelected = false;
    private bool isHighlighted = false;
    
    // Property to check if tile has a turret
    public bool HasTurret { get { return placedTurret != null; } }
    // Property to get the placed turret
    public GameObject PlacedTurret { get { return placedTurret; } }
    // Property to check if tile is selected
    public bool IsSelected { get { return isSelected; } }
    
    // Refund percentage when removing a turret (0.75 = 75%)
    public float turretRefundPercentage = 0.75f;
    
    void Awake()
    {
        tileRenderer = GetComponent<Renderer>();
        if (tileRenderer != null)
        {
            // Store original material, color and position
            originalMaterial = tileRenderer.material;
            originalColor = originalMaterial.color;
            originalPosition = transform.position;
            
            // Set initial color based on buildability
            if (canBuild)
            {
                UpdateTileAppearance();
            }
            
            Debug.Log($"BuildableTile initialized: {gameObject.name}, Layer: {LayerMask.LayerToName(gameObject.layer)}, CanBuild: {canBuild}");
        }
        else
        {
            Debug.LogError($"BuildableTile {gameObject.name} has no Renderer component!");
        }
    }
    
    private void OnDestroy()
    {
        // Clean up the instantiated material to avoid memory leaks
        if (tileRenderer != null && tileRenderer.material != originalMaterial)
        {
            Destroy(tileRenderer.material);
        }
    }
    
    // Called when the tile is selected for building
    public void OnTileSelected()
    {
        if (!canBuild) 
        {
            Debug.Log($"Tile {gameObject.name} cannot be built on.");
            return;
        }
        
        Debug.Log($"Tile {gameObject.name} selected! Changing color to {selectedColor}");
        isSelected = true;
        isHighlighted = false; // Selection takes precedence over highlight
        UpdateTileAppearance();
        OnSelected?.Invoke();
    }
    
    // Called when the tile is deselected
    public void Deselect()
    {
        Debug.Log($"Tile {gameObject.name} deselected.");
        isSelected = false;
        transform.position = originalPosition; // Return to original position
        UpdateTileAppearance();
    }
    
    // Called when the player is hovering over this tile
    public void Highlight()
    {
        if (isSelected) return; // Don't highlight if already selected
        
        isHighlighted = true;
        UpdateTileAppearance();
        
        // Raise the tile slightly to indicate hovering
        if (!isSelected)
        {
            transform.position = originalPosition + Vector3.up * hoverHeight;
        }
    }
    
    // Called when the player stops hovering over this tile
    public void ResetHighlight()
    {
        if (isSelected) return; // Don't change if selected
        
        isHighlighted = false;
        UpdateTileAppearance();
        
        // Return to original position
        if (!isSelected)
        {
            transform.position = originalPosition;
        }
    }
    
    // Try to place a turret on this tile
    public bool PlaceTurret(GameObject turretPrefab)
    {
        if (!canBuild || placedTurret != null)
        {
            Debug.Log($"Cannot place turret on {gameObject.name}: CanBuild={canBuild}, HasTurret={HasTurret}");
            return false;
        }
        
        // Position above the tile with proper height offset
        // Using a larger offset to ensure the turret base sits visibly on the tile
        Vector3 position = transform.position + new Vector3(0, 2.5f, 0);
        
        // Use the tile's rotation but zeroed on the X and Z axes to ensure turret is upright
        Quaternion rotation = Quaternion.Euler(0, transform.rotation.eulerAngles.y, 0);
        
        // Instantiate the turret with the corrected position and rotation
        placedTurret = Instantiate(turretPrefab, position, rotation);
        
        Debug.Log($"Turret placed on {gameObject.name} at position {position}, rotation {rotation.eulerAngles}");
        
        // Update tile appearance
        isSelected = false; // No longer selected after placement
        isHighlighted = false;
        transform.position = originalPosition; // Reset position
        UpdateTileAppearance();
        
        // Fire event
        OnTurretPlaced?.Invoke(placedTurret);
        
        return true;
    }
    
    // Remove a turret from this tile and refund currency
    public bool RemoveTurret()
    {
        if (placedTurret == null)
        {
            Debug.Log($"No turret to remove from {gameObject.name}");
            return false;
        }
        
        Destroy(placedTurret);
        placedTurret = null;
        
        Debug.Log($"Turret removed from {gameObject.name}");
        
        // Update tile appearance
        UpdateTileAppearance();
        
        // Fire event
        OnTurretRemoved?.Invoke();
        
        return true;
    }
    
    // Change whether this tile can be built on
    public void SetBuildability(bool canBuildOnTile)
    {
        if (canBuild != canBuildOnTile)
        {
            canBuild = canBuildOnTile;
            UpdateTileAppearance();
            Debug.Log($"Buildability of {gameObject.name} set to {canBuild}");
        }
    }
    
    // Update the tile's visual appearance based on its state
    private void UpdateTileAppearance()
    {
        if (tileRenderer == null) return;
        
        // Create a new material instance if needed to avoid affecting other objects
        if (tileRenderer.material == originalMaterial)
        {
            tileRenderer.material = new Material(originalMaterial);
        }
        
        // Determine the color based on state
        if (!canBuild)
        {
            // Not buildable - use original color
            tileRenderer.material.color = originalColor;
        }
        else if (isSelected)
        {
            // Selected state
            tileRenderer.material.color = selectedColor;
            transform.position = originalPosition + Vector3.up * selectionHeight;
        }
        else if (placedTurret != null)
        {
            // Has turret
            tileRenderer.material.color = occupiedColor;
        }
        else if (isHighlighted)
        {
            // Highlighted (hovered over)
            tileRenderer.material.color = highlightColor;
        }
        else
        {
            // Available for building
            tileRenderer.material.color = availableColor;
        }
    }
} 