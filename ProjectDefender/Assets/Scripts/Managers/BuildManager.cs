using System.Collections.Generic;
using UnityEngine;

public class BuildManager : MonoBehaviour
{
   private UI ui;
   public BuildSlot selectedBuildSlot;

   public WaveManager waveManager;
   public GridBuilder currentGrid;
   private GameManager gameManager;
   private CameraEffects cameraEffects;

   [SerializeField] private LayerMask whatToIgnore;
   
   [Header("Build Materials")]
   [SerializeField] private Material attackRadiusMat;
   [SerializeField] private Material buildPreviewMat;
   
   [Header("Build Details")]
   [SerializeField] private float towerCentreY = .5f;
   [SerializeField] private float camShakeDuration = .02f;
   [SerializeField] private float camShakeMagnitude = .15f;
   [SerializeField] private AudioClip buildSound;

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

   public void UpdateBuildManager(WaveManager newWaveManager, GridBuilder newCurrentGrid)
   {
      currentGrid = newCurrentGrid;
      MakeBuildSlotNotAvailableIfNeeded(newWaveManager, currentGrid);
   }

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
      
      if (buildSound != null && AudioManager.instance != null)
      {
         AudioManager.instance.PlaySFXOneShot(buildSound, buildPosition, true);
      }
      
      ForwardAttackDisplay display = newTower.GetComponent<ForwardAttackDisplay>();
      if (display != null)
      {
         display.UpdateLines();
      }
      
      gameManager.UpdateCurrency(-towerPrice);
      
      // Clean up the slot selection
      slotToUse.UnselectTile();
      selectedBuildSlot = null;
   }

   public void MouseOverUI(bool isOverUI) => isMouseOverUI = isOverUI;
   
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

   public void SelectBuildSlot(BuildSlot newSlot)
   {
      if (selectedBuildSlot != null) selectedBuildSlot.UnselectTile();
    
      selectedBuildSlot = newSlot;
      
      if (ui != null && ui.BuildButtonsHolderUI != null)
      {
         ui.BuildButtonsHolderUI.OnTileSelectionChanged(newSlot);
      }
   }

   public void EnableBuildMenu()
   {
      if (buildMenuEnabled) return;
    
      buildMenuEnabled = true;
      ui.BuildButtonsHolderUI.ShowBuildButtons(true);
   }

   public void DisableBuildMenu()
   {
      if (!buildMenuEnabled) return;
    
      buildMenuEnabled = false;
      ui.BuildButtonsHolderUI.ShowBuildButtons(false);
   }

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

   public void DisableSellMenu()
   {
      if (!sellMenuEnabled) return;
    
      sellMenuEnabled = false;
      ui.inGameUI.EnableSellTowerUI(false);
   }

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

   public bool HasTowerOnSlot(BuildSlot slot)
   {
      return builtTowers.ContainsKey(slot);
   }

   public int GetTowerSellValue(BuildSlot slot)
   {
      if (!HasTowerOnSlot(slot)) return 0;
      
      return Mathf.RoundToInt(builtTowers[slot].originalPrice * 0.5f);
   }

   public BuildSlot GetSelectedSlot() => selectedBuildSlot;
   public Material GetAttackRadiusMat() => attackRadiusMat;
   public Material GetBuildPreviewMat() => buildPreviewMat;
}

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