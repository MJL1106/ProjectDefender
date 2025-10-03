
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
   

   private bool isMouseOverUI;
   
   private void Awake()
   {
      ui = FindFirstObjectByType<UI>();
      cameraEffects = FindFirstObjectByType<CameraEffects>();
      
     // MakeBuildSlotNotAvailableIfNeeded(waveManager,currentGrid);
   }

   private void Update()
   {
      if (Input.GetKeyDown(KeyCode.Escape)) CancelBuildAction();

      if (Input.GetKeyDown(KeyCode.Mouse0) && isMouseOverUI == false)
      {
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

   private void Start()
   {
      gameManager = GameManager.instance;
   }

   public void BuildTower(GameObject towerToBuild, int towerPrice, Transform newPreviewTower)
   {
      if (gameManager.HasEnoughCurrency(towerPrice) == false)
      {
         ui.inGameUI.ShakeCurrencyUI();
         return;
      }
        
      if (towerToBuild == null)
      {
         Debug.LogWarning("YOu did not assign a tower to this button!");
         return;
      }

      if (ui.BuildButtonsHolderUI.GetLastSelected() == null)
      {
         return;
      }

      Transform previewTower = newPreviewTower;
      BuildSlot slotToUse = GetSelectedSlot();
      CancelBuildAction();
        
      slotToUse.SnapToDefaultPosition();
      slotToUse.SetSlotAvailableTo(false);
        
      ui.BuildButtonsHolderUI.SetLastSelected(null, null);
        
      cameraEffects.ScreenShake(camShakeDuration, camShakeMagnitude);

      GameObject newTower = Instantiate(towerToBuild,slotToUse.GetBuildPosition(towerCentreY), Quaternion.identity);
      newTower.transform.rotation = previewTower.rotation;
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
   }

   public void SelectBuildSlot(BuildSlot newSlot)
   {
      if (selectedBuildSlot != null) selectedBuildSlot.UnselectTile();
    
      selectedBuildSlot = newSlot;
    
      // Update the preview position if there's an active button selected
      if (ui != null && ui.BuildButtonsHolderUI != null)
      {
         ui.BuildButtonsHolderUI.OnTileSelectionChanged(newSlot);
      }
   }

   public void EnableBuildMenu()
   {
      if (selectedBuildSlot != null) return;
      
      ui.BuildButtonsHolderUI.ShowBuildButtons(true);
   }

   private void DisableBuildMenu()
   {
      ui.BuildButtonsHolderUI.ShowBuildButtons(false);
   }

   public BuildSlot GetSelectedSlot() => selectedBuildSlot;
   public Material GetAttackRadiusMat() => attackRadiusMat;
   public Material GetBuildPreviewMat() => buildPreviewMat;
}
