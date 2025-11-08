using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// Manages all tower building and selling logic.
/// Handles build slot selection, UI interaction, and tracking built towers.
/// </summary>
public class BuildManager : MonoBehaviour
{
   private UI ui;
   public BuildSlot selectedBuildSlot;

   public WaveManager waveManager;
   public GridBuilder currentGrid;
   private GameManager gameManager;
   private CameraEffects cameraEffects;

   [SerializeField] private LayerMask whatToIgnore; // Layers to ignore when raycasting for build slots
   
   [Header("Build Materials")]
   [SerializeField] private Material attackRadiusMat;
   [SerializeField] private Material buildPreviewMat;
   
   [Header("Build Details")]
   [SerializeField] private float towerCentreY = .5f; // Vertical offset for placing the tower model
   [SerializeField] private float camShakeDuration = .02f;
   [SerializeField] private float camShakeMagnitude = .15f;
   [SerializeField] private AudioClip buildSound;
   [SerializeField] private AudioClip sellSound;
   [SerializeField] private float buildSoundVolume = 1f;
   [SerializeField] private float sellSoundVolume = 1f;

   private bool sellMenuEnabled = false;
   private bool buildMenuEnabled = false;
   
   // Tower tracking
   private Dictionary<BuildSlot, TowerData> builtTowers = new Dictionary<BuildSlot, TowerData>();

   private bool isMouseOverUI;
   
   private void Awake()
   {
      ui = FindFirstObjectByType<UI>();
      cameraEffects = FindFirstObjectByType<CameraEffects>();
      
     MakeBuildSlotNotAvailableIfNeeded(waveManager,currentGrid);
   }
   
   private void Start()
   {
      gameManager = GameManager.instance;
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Escape)) CancelBuildAction();

      if (Input.GetKeyDown(KeyCode.Mouse0))
      {
         if (isMouseOverUI) return;
         
         if (Physics.Raycast(Camera.main.ScreenPointToRay(Input.mousePosition), out RaycastHit hit, Mathf.Infinity, ~whatToIgnore))
         {
            bool clickedNotOnBuildSlot = hit.collider.GetComponent<BuildSlot>() == null;
            
            if (clickedNotOnBuildSlot) CancelBuildAction();
         }
      }
   }

   /// <summary>
   /// Called by LevelSetup to provide the level-specific WaveManager and Grid.
   /// </summary>
   public void UpdateBuildManager(WaveManager newWaveManager, GridBuilder newCurrentGrid)
   {
      currentGrid = newCurrentGrid;
      MakeBuildSlotNotAvailableIfNeeded(newWaveManager, currentGrid);
   }

   /// <summary>
   /// Builds a tower on the currently selected slot.
   /// Spends currency, plays effects, and tracks the new tower.
   /// </summary>
   public void BuildTower(GameObject towerToBuild, int towerPrice, Transform newPreviewTower)
   {
      if (gameManager.HasEnoughCurrency(towerPrice) == false)
      {
         ui.inGameUI.ShakeCurrencyUI();
         return;
      }
     
      if (towerToBuild == null) return;
   
      if (ui.BuildButtonsHolderUI.GetLastSelected() == null) return;
   
      Transform previewTower = newPreviewTower;
      BuildSlot slotToUse = GetSelectedSlot();
   
      // Manually hide the preview and build menu
      ui.BuildButtonsHolderUI.GetLastSelected()?.SelectButton(false);
      DisableBuildMenu();
     
      slotToUse.SnapToDefaultPosition();
      slotToUse.SetSlotAvailableTo(false);
     
      ui.BuildButtonsHolderUI.SetLastSelected(null, null);
     
      cameraEffects.ScreenShake(camShakeDuration, camShakeMagnitude);

      Vector3 buildPosition = slotToUse.GetBuildPosition(towerCentreY);

      GameObject newTower = Instantiate(towerToBuild, buildPosition, Quaternion.identity);
      newTower.transform.rotation = previewTower.rotation;
   
      // Track the built tower
      builtTowers[slotToUse] = new TowerData(newTower, towerPrice);
   
      // Play build sound at the tower's position
      if (buildSound != null && AudioManager.instance != null)
         AudioManager.instance.PlaySFXOneShot(buildSound, buildPosition, true, buildSoundVolume);
      
   
      ForwardAttackDisplay display = newTower.GetComponent<ForwardAttackDisplay>();
      if (display != null) display.UpdateLines();
      
      gameManager.UpdateCurrency(-towerPrice);
   
      // Clean up the slot selection
      slotToUse.UnselectTile();
      selectedBuildSlot = null;
   }

   /// <summary>
   /// Used by UI event triggers to block building when the mouse is over UI.
   /// </summary>
   public void MouseOverUI(bool isOverUI) => isMouseOverUI = isOverUI;
   
   /// <summary>
   /// Scans upcoming wave data to disable build slots on tiles that will be replaced.
   /// </summary>
   public void MakeBuildSlotNotAvailableIfNeeded(WaveManager newWaveManager, GridBuilder myCurrentGrid)
   {
      if (newWaveManager == null) return;
      
      foreach (var wave in newWaveManager.GetLevelWaves())
      {
         if (wave.nextGrid == null) continue;
         
         List<GameObject> grid = myCurrentGrid.GetTileSetup();
         List<GameObject> nextWaveGrid = wave.nextGrid.GetTileSetup();

         for (int i = 0; i < grid.Count; i++)
         {
            TileSlot currentTile = grid[i].GetComponent<TileSlot>();
            TileSlot nextTile = nextWaveGrid[i].GetComponent<TileSlot>();

            bool tileNotTheSame = currentTile.GetMesh() != nextTile.GetMesh() ||
                                  currentTile.GetOriginalMaterial() != nextTile.GetOriginalMaterial() ||
                                  currentTile.GetAllChildren().Count != nextTile.GetAllChildren().Count;

            if (tileNotTheSame == false) continue;
            
            BuildSlot buildSlot = grid[i].GetComponent<BuildSlot>();
            
            if (buildSlot != null) buildSlot.SetSlotAvailableTo(false);
         }
      }
   }

   /// <summary>
   /// Resets any active build or sell action and deselects the current slot.
   /// </summary>
   public void CancelBuildAction()
   {
      if (selectedBuildSlot == null) return;
    
      ui.BuildButtonsHolderUI.GetLastSelected()?.SelectButton(false);
    
      selectedBuildSlot.UnselectTile();
      selectedBuildSlot = null;
    
      DisableBuildMenu();
      DisableSellMenu();
    
      buildMenuEnabled = false;
      sellMenuEnabled = false;
   }

   /// <summary>
   /// Called by a BuildSlot when it is clicked.
   /// Manages deselection of old slot and selection of the new one.
   /// </summary>
   public void SelectBuildSlot(BuildSlot newSlot)
   {
      if (selectedBuildSlot != null) selectedBuildSlot.UnselectTile();
    
      selectedBuildSlot = newSlot;
      
      if (ui != null && ui.BuildButtonsHolderUI != null) ui.BuildButtonsHolderUI.OnTileSelectionChanged(newSlot);
      
   }

   /// <summary>
   /// Shows the tower build/upgrade UI panel.
   /// </summary>
   public void EnableBuildMenu()
   {
      if (buildMenuEnabled) return;
    
      buildMenuEnabled = true;
      ui.BuildButtonsHolderUI.ShowBuildButtons(true);
   }

   /// <summary>
   /// Hides the tower build/upgrade UI panel.
   /// </summary>
   public void DisableBuildMenu()
   {
      if (!buildMenuEnabled) return;
    
      buildMenuEnabled = false;
      ui.BuildButtonsHolderUI.ShowBuildButtons(false);
   }

   /// <summary>
   /// Shows the sell tower UI panel with the correct sell value.
   /// </summary>
   public void EnableSellMenu()
   {
      if (selectedBuildSlot == null || !HasTowerOnSlot(selectedBuildSlot)) return;

      int sellValue = GetTowerSellValue(selectedBuildSlot);
    
      // If already enabled, just update the value without animating
      if (sellMenuEnabled)
      {
         ui.inGameUI.UpdateSellTowerValue(sellValue);
         return;
      }
    
      sellMenuEnabled = true;
      ui.inGameUI.EnableSellTowerUI(true, sellValue);
   }

   /// <summary>
   /// Hides the sell tower UI panel.
   /// </summary>
   public void DisableSellMenu()
   {
      if (!sellMenuEnabled) return;
    
      sellMenuEnabled = false;
      ui.inGameUI.EnableSellTowerUI(false);
   }

   /// <summary>
   /// Sells the tower on the currently selected slot.
   /// Gives currency, plays effects, and makes the slot available again.
   /// </summary>
   public void SellSelectedTower()
   {
      if (selectedBuildSlot == null || !HasTowerOnSlot(selectedBuildSlot)) return;

      int sellValue = GetTowerSellValue(selectedBuildSlot);
      BuildSlot slotBeingSold = selectedBuildSlot;

      cameraEffects.ScreenShake(camShakeDuration, camShakeMagnitude);
   
      // Destroy the tower
      TowerData towerData = builtTowers[slotBeingSold];
      if (towerData.towerObject != null)
      {
         Vector3 towerPosition = towerData.towerObject.transform.position;
      
         // Play sell sound at tower position before destroying
         if (sellSound != null && AudioManager.instance != null) 
            AudioManager.instance.PlaySFXOneShot(sellSound, towerPosition, true, sellSoundVolume);
      
         Destroy(towerData.towerObject);
      }
   
      // Remove from tracking
      builtTowers.Remove(slotBeingSold);
   
      // Make slot available again
      slotBeingSold.SetSlotAvailableTo(true);
   
      // Add currency back to player
      gameManager.UpdateCurrency(sellValue);
   
      // Hide sell UI and deselect
      CancelBuildAction();
   }

   /// <summary>
   /// Checks if a tower is currently tracked on the given slot.
   /// </summary>
   public bool HasTowerOnSlot(BuildSlot slot)
   {
      return builtTowers.ContainsKey(slot);
   }

   /// <summary>
   /// Calculates the 50% sell value for a tower on the given slot.
   /// </summary>
   public int GetTowerSellValue(BuildSlot slot)
   {
      if (!HasTowerOnSlot(slot)) return 0;
      
      return Mathf.RoundToInt(builtTowers[slot].originalPrice * 0.5f);
   }

   public BuildSlot GetSelectedSlot() => selectedBuildSlot;
   public Material GetAttackRadiusMat() => attackRadiusMat;
   public Material GetBuildPreviewMat() => buildPreviewMat;
}

/// <summary>
/// A simple data class to track a built tower and its original price.
/// </summary>
[System.Serializable]
public class TowerData
{
   public GameObject towerObject;
   public int originalPrice;

   public TowerData(GameObject tower, int price)
   {
      towerObject = tower;
      originalPrice = price;
   }
}