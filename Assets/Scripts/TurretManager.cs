using System.Collections.Generic;
using UnityEngine;
using TMPro;

public class TurretManager : MonoBehaviour
{
    [System.Serializable]
    public class TurretData
    {
        public string name;
        public GameObject prefab;
        public Sprite icon;
    }
    
    public List<TurretData> availableTurrets = new List<TurretData>();
    
    [Header("Economy")]
    public TextMeshProUGUI currencyText;
    
    // Property to access GameManager's currency
    public int PlayerCurrency 
    { 
        get 
        {
            if (GameManager.Instance != null)
                return GameManager.Instance.Currency;
            return 0;
        }
    }
    
    [Header("UI")]
    public GameObject turretSelectionUI;
    
    private BuildableTile selectedTile;
    private TurretData selectedTurret;
    private GameManager gameManager;
    
    // Property to check if a tile is currently selected
    public bool HasSelectedTile => selectedTile != null;

    // Method to get the currently selected tile (used by TurretShopUI)
    public BuildableTile GetSelectedTile()
    {
        return selectedTile;
    }
    
    void Start()
    {
        gameManager = GameManager.Instance;
        if (gameManager == null)
        {
            Debug.LogError("TurretManager couldn't find GameManager instance!");
        }
        
        UpdateCurrencyUI();
        
        // Hide turret selection UI initially
        if (turretSelectionUI != null)
        {
            turretSelectionUI.SetActive(false);
        }
        else
        {
            Debug.LogError("TurretManager: turretSelectionUI is not assigned!");
        }
        
        Debug.Log($"TurretManager initialized with {availableTurrets.Count} turret types and {PlayerCurrency} currency");
    }
    
    // Called by BuildableTile or PlayerTools to select a tile for building
    public void SelectTile(BuildableTile tile)
    {
        Debug.Log($"TurretManager.SelectTile called with tile: {(tile != null ? tile.name : "null")}");
        
        // If a different tile was previously selected, deselect it
        if (selectedTile != null && selectedTile != tile)
        {
            Debug.Log("Deselecting previous tile");
            selectedTile.Deselect();
        }
        
        // Store the reference and notify the tile it's selected
        selectedTile = tile;
        selectedTile.OnTileSelected();
        
        // Show turret selection UI
        if (turretSelectionUI != null)
        {
            Debug.Log("Showing turret selection UI");
            turretSelectionUI.SetActive(true);
            
            // Position UI near the tile or in a fixed screen position
            // This depends on your UI setup
            
            // Always update shop UI when selecting a new tile
            UpdateShopUI();
        }
        else
        {
            Debug.LogError("Cannot show turret selection UI - it's not assigned!");
        }
    }
    
    // Called by TurretShopUI when a turret type is selected
    public void SelectTurretForPlacement(int turretIndex)
    {
        Debug.Log($"TurretManager.SelectTurretForPlacement called with index: {turretIndex}");
        
        if (turretIndex >= 0 && turretIndex < availableTurrets.Count)
        {
            selectedTurret = availableTurrets[turretIndex];
            
            // Check if player has enough currency
            Turret turretComponent = selectedTurret.prefab.GetComponent<Turret>();
            if (turretComponent != null && PlayerCurrency >= turretComponent.cost)
            {
                Debug.Log($"Selected turret: {selectedTurret.name}, Cost: {turretComponent.cost}");
                
                // Keep UI open after placement
                if (selectedTile != null)
                {
                    if (selectedTile.HasTurret)
                    {
                        Debug.Log("Tile already has a turret");
                        return;
                    }
                    
                    if (selectedTile.PlaceTurret(selectedTurret.prefab))
                    {
                        Debug.Log($"Successfully placed turret {selectedTurret.name} on tile {selectedTile.name}");
                        
                        // Deduct cost
                        gameManager.SpendCurrency(turretComponent.cost);
                        UpdateCurrencyUI();
                        
                        // Update the shop UI (which will update button states based on the new situation)
                        UpdateShopUI();
                        
                        // Let's keep the UI open and keep the tile selected
                    }
                    else
                    {
                        Debug.LogError($"Failed to place turret on tile {selectedTile.name}");
                    }
                }
                else
                {
                    Debug.Log("No tile selected for turret placement");
                }
            }
            else
            {
                // Not enough currency
                Debug.Log($"Not enough currency to purchase this turret! Have: {PlayerCurrency}, Need: {turretComponent?.cost}");
            }
        }
        else
        {
            Debug.LogError($"Invalid turret index: {turretIndex}. Available turrets: {availableTurrets.Count}");
        }
    }
    
    // Cancel the current placement
    public void CancelPlacement()
    {
        Debug.Log("TurretManager.CancelPlacement called");
        
        // Deselect the tile
        if (selectedTile != null)
        {
            Debug.Log($"Deselecting tile {selectedTile.name}");
            selectedTile.Deselect();
            selectedTile = null;
        }
        
        // Clear the selected turret
        selectedTurret = null;
        
        // Hide the UI when canceling
        if (turretSelectionUI != null)
        {
            turretSelectionUI.SetActive(false);
        }
    }
    
    public void AddCurrency(int amount)
    {
        // Forward the call to GameManager instead of managing currency internally
        if (gameManager != null)
        {
            gameManager.AddCurrency(amount);
        }
        else
        {
            Debug.LogError("TurretManager: Cannot add currency, GameManager is null!");
        }
    }
    
    public void UpdateCurrencyUI()
    {
        if (currencyText != null)
        {
            currencyText.text = $"{PlayerCurrency} Cals";
        }
        else
        {
            Debug.LogWarning("TurretManager: currencyText is not assigned");
        }
        
        // We're removing this part to prevent enemy kills from affecting button states
        // The UpdateShopUI method will handle button states separately when needed
    }
    
    // Sell a turret and get refund
    public void SellTurret(BuildableTile tile)
    {
        if (tile == null || !tile.HasTurret) 
        {
            Debug.LogWarning("Attempted to sell a turret, but the tile is null or has no turret");
            return;
        }
        
        // Get the turret component to calculate refund
        Turret turretComponent = tile.PlacedTurret.GetComponent<Turret>();
        if (turretComponent != null)
        {
            // Calculate refund (2/3 of cost, rounded to nearest 5)
            float refundExact = turretComponent.cost * (2.0f/3.0f);
            int refundAmount = Mathf.RoundToInt(refundExact / 5) * 5; // Round to nearest 5
            
            // Add currency
            AddCurrency(refundAmount);
            Debug.Log($"Refunded {refundAmount} Cals (2/3 of {turretComponent.cost}, rounded to nearest 5)");
        }
        
        // Store whether this was the selected tile
        bool wasSelectedTile = (tile == selectedTile);
        
        // Remove the turret
        tile.RemoveTurret();
        
        // If this was the selected tile, make sure it remains selected and the UI stays open
        if (wasSelectedTile)
        {
            // Force an update of the shop UI to reflect the new state (no turret)
            UpdateShopUI();
        }
    }
    
    // Check if player can afford a turret
    public bool CanAffordTurret(int cost)
    {
        return PlayerCurrency >= cost;
    }
    
    // Get the cost of a turret at the specified index
    public int GetTurretCost(int turretIndex)
    {
        if (turretIndex >= 0 && turretIndex < availableTurrets.Count)
        {
            Turret turretComponent = availableTurrets[turretIndex].prefab.GetComponent<Turret>();
            if (turretComponent != null)
            {
                return turretComponent.cost;
            }
        }
        return 0;
    }
    
    // Attempt to place a specific turret prefab
    public void AttemptToPlaceTurret(GameObject turretPrefab)
    {
        if (selectedTile == null) return;
        
        // Check cost
        Turret turretComponent = turretPrefab.GetComponent<Turret>();
        if (turretComponent != null)
        {
            int cost = turretComponent.cost;
            
            if (PlayerCurrency >= cost)
            {
                // Deduct currency
                gameManager.SpendCurrency(cost);
                
                // Place turret
                selectedTile.PlaceTurret(turretPrefab);
                
                // Reset placement state
                selectedTile = null;
                
                // Update UI
                UpdateCurrencyUI();
            }
            else
            {
                Debug.Log("Not enough currency to build turret!");
            }
        }
    }
    
    // Helper method to update the shop UI
    private void UpdateShopUI()
    {
        // Find and update the TurretShopUI
        TurretShopUI shopUI = FindObjectOfType<TurretShopUI>();
        if (shopUI != null && shopUI.gameObject.activeInHierarchy)
        {
            shopUI.UpdateButtonsState();
        }
    }
} 