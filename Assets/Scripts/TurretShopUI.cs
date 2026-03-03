using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class TurretShopUI : MonoBehaviour
{
    public TurretManager turretManager;
    
    [Header("Turret Buttons")]
    public Button machineGunButton;
    public Button sniperButton; 
    public Button artilleryButton;
    public Button removeButton; // Button for removing turrets
    
    [Header("Button Text")]
    public TextMeshProUGUI machineGunCostText;
    public TextMeshProUGUI sniperCostText;
    public TextMeshProUGUI artilleryCostText;
    public TextMeshProUGUI removeButtonText; // Text for the remove button
    
    [Header("UI Controls")]
    public Button closeButton;
    
    // Indices of turret types in TurretManager's availableTurrets list
    [Header("Turret Indices")]
    public int machineGunIndex = 0;
    public int sniperIndex = 1;
    public int artilleryIndex = 2;
    
    private void Awake()
    {
        // Make sure the shop starts hidden
        gameObject.SetActive(false);
    }
    
    void OnEnable()
    {
        if (turretManager == null)
        {
            turretManager = FindObjectOfType<TurretManager>();
        }
        
        if (turretManager != null)
        {
            SetupButtons();
            UpdateButtonsState();
        }
        else
        {
            Debug.LogError("TurretShopUI: No TurretManager found in the scene!");
        }
    }
    
    void SetupButtons()
    {
        // Set up Machine Gun button
        if (machineGunButton != null)
        {
            machineGunButton.onClick.RemoveAllListeners();
            machineGunButton.onClick.AddListener(() => SelectTurret(machineGunIndex));
            
            // Update text if available
            if (machineGunCostText != null && machineGunIndex < turretManager.availableTurrets.Count)
            {
                int cost = GetTurretCost(machineGunIndex);
                string turretName = turretManager.availableTurrets[machineGunIndex].name;
                machineGunCostText.text = $"{turretName}: {cost} Cals";
            }
        }
        
        // Set up Sniper button
        if (sniperButton != null)
        {
            sniperButton.onClick.RemoveAllListeners();
            sniperButton.onClick.AddListener(() => SelectTurret(sniperIndex));
            
            // Update text if available
            if (sniperCostText != null && sniperIndex < turretManager.availableTurrets.Count)
            {
                int cost = GetTurretCost(sniperIndex);
                string turretName = turretManager.availableTurrets[sniperIndex].name;
                sniperCostText.text = $"{turretName}: {cost} Cals";
            }
        }
        
        // Set up Artillery button
        if (artilleryButton != null)
        {
            artilleryButton.onClick.RemoveAllListeners();
            artilleryButton.onClick.AddListener(() => SelectTurret(artilleryIndex));
            
            // Update text if available
            if (artilleryCostText != null && artilleryIndex < turretManager.availableTurrets.Count)
            {
                int cost = GetTurretCost(artilleryIndex);
                string turretName = turretManager.availableTurrets[artilleryIndex].name;
                artilleryCostText.text = $"{turretName}: {cost} Cals";
            }
        }
        
        // Set up Remove Turret button
        if (removeButton != null)
        {
            removeButton.onClick.RemoveAllListeners();
            removeButton.onClick.AddListener(RemoveTurret);
            
            if (removeButtonText != null)
            {
                removeButtonText.text = "Remove Turret";
                removeButtonText.color = Color.red; // Always red text
            }
        }
        
        // Set up close button
        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(() => {
                if (turretManager != null)
                {
                    turretManager.CancelPlacement();
                }
            });
        }
    }
    
    public void UpdateButtonsState()
    {
        // Get the currently selected tile
        BuildableTile selectedTile = null;
        bool hasTurret = false;
        
        // Check if a tile is selected
        if (turretManager != null && turretManager.HasSelectedTile)
        {
            selectedTile = turretManager.GetSelectedTile();
            if (selectedTile != null)
            {
                hasTurret = selectedTile.HasTurret;
            }
        }
        
        // Update machine gun button
        if (machineGunButton != null && machineGunIndex < turretManager.availableTurrets.Count)
        {
            int cost = GetTurretCost(machineGunIndex);
            // Enable if we have enough currency and either no tile is selected or there's no turret on the selected tile
            bool canAfford = turretManager.PlayerCurrency >= cost;
            bool canPlace = selectedTile == null || !hasTurret;
            machineGunButton.interactable = canAfford && canPlace;
            
            // Update text color for affordability
            if (machineGunCostText != null)
            {
                machineGunCostText.color = canAfford ? Color.green : Color.red;
            }
        }
        
        // Update sniper button
        if (sniperButton != null && sniperIndex < turretManager.availableTurrets.Count)
        {
            int cost = GetTurretCost(sniperIndex);
            // Enable if we have enough currency and either no tile is selected or there's no turret on the selected tile
            bool canAfford = turretManager.PlayerCurrency >= cost;
            bool canPlace = selectedTile == null || !hasTurret;
            sniperButton.interactable = canAfford && canPlace;
            
            // Update text color for affordability
            if (sniperCostText != null)
            {
                sniperCostText.color = canAfford ? Color.green : Color.red;
            }
        }
        
        // Update artillery button
        if (artilleryButton != null && artilleryIndex < turretManager.availableTurrets.Count)
        {
            int cost = GetTurretCost(artilleryIndex);
            // Enable if we have enough currency and either no tile is selected or there's no turret on the selected tile
            bool canAfford = turretManager.PlayerCurrency >= cost;
            bool canPlace = selectedTile == null || !hasTurret;
            artilleryButton.interactable = canAfford && canPlace;
            
            // Update text color for affordability
            if (artilleryCostText != null)
            {
                artilleryCostText.color = canAfford ? Color.green : Color.red;
            }
        }
        
        // Update remove turret button - only active if there's a turret on the selected tile
        if (removeButton != null)
        {
            removeButton.interactable = selectedTile != null && hasTurret;
            
            // Text is always red, as specified
            if (removeButtonText != null)
            {
                removeButtonText.color = Color.red;
            }
        }
    }
    
    // Method to remove a turret from the selected tile
    public void RemoveTurret()
    {
        if (turretManager != null && turretManager.HasSelectedTile)
        {
            BuildableTile selectedTile = turretManager.GetSelectedTile();
            if (selectedTile != null && selectedTile.HasTurret)
            {
                turretManager.SellTurret(selectedTile);
                // Don't call CancelPlacement which deselects the tile
                // This way the UI stays open and the player can build a new turret on the same tile
                
                // Instead, just update the button states
                UpdateButtonsState();
            }
            else
            {
                Debug.Log("No turret to remove on the selected tile.");
            }
        }
    }
    
    public void SelectTurret(int index)
    {
        if (turretManager != null && index < turretManager.availableTurrets.Count)
        {
            turretManager.SelectTurretForPlacement(index);
            // The UI now stays open after placement
            // Update the button states to reflect the new situation
            UpdateButtonsState();
        }
    }
    
    // Helper method to get cost from turret component
    private int GetTurretCost(int index)
    {
        if (turretManager != null && index < turretManager.availableTurrets.Count)
        {
            GameObject prefab = turretManager.availableTurrets[index].prefab;
            if (prefab != null)
            {
                Turret turret = prefab.GetComponent<Turret>();
                if (turret != null)
                {
                    return turret.cost;
                }
            }
        }
        return 0;
    }
    
    // Call this from other scripts (like Update method in CameraToggle) to keep UI updated
    public void RefreshUI()
    {
        if (gameObject.activeInHierarchy && turretManager != null)
        {
            UpdateButtonsState();
        }
    }
} 