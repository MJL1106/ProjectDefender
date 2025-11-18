using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Manages all in-game UI elements.
/// Handles updating health, currency, wave timers, and sell/victory/defeat panels.
/// </summary>
public class UIGame : MonoBehaviour
{
    private UI ui;
    private UIPause pauseUI;
    
    private UIAnimator animatorUI;
    
    [SerializeField] private TextMeshProUGUI healthPointsText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [Space]
    [SerializeField] private TextMeshProUGUI waveTimerText;
    [SerializeField] private float waveTimerOffset;
    [SerializeField] private UITextBlinkEffect waveTimerTextBlinkEffect;

    [SerializeField] private Transform waveTimer;
    private Coroutine waveTimerMoveCo;
    private Vector3 waveTimerDefaultPosition;

    [Header("Sell Tower UI")]
    [SerializeField] private Transform sellTowerUI;
    [SerializeField] private float sellTowerOffset;
    [SerializeField] private UITextBlinkEffect sellTowerTextBlinkEffect;
    [SerializeField] private TextMeshProUGUI sellTowerValueText;
    private Coroutine sellTowerMoveCo;
    private Vector3 sellTowerDefaultPosition;

    [Header("Next Wave Details UI")]
    [SerializeField] private Transform nextWaveDetailsUI;
    [SerializeField] private float nextWaveDetailsOffset;
    [SerializeField] private TextMeshProUGUI waveNumberText;
    [SerializeField] private TextMeshProUGUI enemyDetailsText;
    [SerializeField] private float detailsBasePadding = 70f; // Base height of the panel
    private Coroutine nextWaveDetailsMoveCo;
    private Vector3 nextWaveDetailsDefaultPosition;
    private Vector2 nextWaveDetailsOriginalSize;
    private WaveManager waveManager;
    
    [Header("Victory and Defeat")] 
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject levelCompletedUI;

    private void Awake()
    {
        animatorUI = GetComponentInParent<UIAnimator>();
        ui = GetComponentInParent<UI>();
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null) pauseUI = canvas.GetComponentInChildren<UIPause>(true);
        
        if (pauseUI == null)
        {
            GameObject pauseUI = GameObject.Find("Pause_UI");
            if (pauseUI != null) this.pauseUI = pauseUI.GetComponent<UIPause>();
        }
        
        if (nextWaveDetailsUI != null) 
        {
            nextWaveDetailsDefaultPosition = nextWaveDetailsUI.localPosition;
            // Store the original size from the prefab
            RectTransform rect = nextWaveDetailsUI.GetComponent<RectTransform>();
            nextWaveDetailsOriginalSize = rect.sizeDelta;
        }

        if (waveTimer != null) waveTimerDefaultPosition = waveTimer.localPosition;
        if (sellTowerUI != null) sellTowerDefaultPosition = sellTowerUI.localPosition;
        if (nextWaveDetailsUI != null) nextWaveDetailsDefaultPosition = nextWaveDetailsUI.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10)) ui.SwitchTo(pauseUI.gameObject);
    }

    /// <summary>
    /// Shows or hides the final game victory panel.
    /// </summary>
    public void EnableVictoryUI(bool enable)
    {
        if (victoryUI != null) victoryUI.SetActive(enable);
    }

    /// <summary>
    /// Shows or hides the game over / defeat panel.
    /// </summary>
    public void EnableGameOverUI(bool enable)
    {
        if (gameOverUI != null) gameOverUI.SetActive(enable);
    }

    /// <summary>
    /// Shows or hides the level complete panel.
    /// </summary>
    public void EnableLevelCompletedUI(bool enable)
    {
        if (levelCompletedUI != null) levelCompletedUI.SetActive(enable);
    }

    /// <summary>
    /// Triggers the shake animation on the currency text.
    /// </summary>
    public void ShakeCurrencyUI()
    {
        ui.animUI.Shake(currencyText.transform.parent);
    }

    /// <summary>
    /// Triggers the shake animation on the health text.
    /// </summary>
    public void ShakeHealthUI()
    {
        ui.animUI.Shake(healthPointsText.transform.parent);
    }

    /// <summary>
    /// Updates the health points text.
    /// </summary>
    /// <param name="value">The current health value.</param>
    /// <param name="maxValue">The maximum health value.</param>
    public void UpdateHealthPointsUI(int value, int maxValue)
    {
        int newValue = maxValue - value;
        healthPointsText.text = "Threat : " + newValue + "/" + maxValue;
    }
    
    /// <summary>
    /// Updates the currency text.
    /// </summary>
    /// <param name="value">The new currency amount.</param>
    public void UpdateCurrencyUI(int value)
    {
        currencyText.text = "resources : " + value;
    }

    /// <summary>
    /// Updates the wave countdown timer text.
    /// </summary>
    /// <param name="value">The time remaining in seconds.</param>
    public void UpdateWaveTimerUI(float value)
    {
        waveTimerText.text = "seconds : " + value.ToString("00");
    }
    
    /// <summary>
    /// Updates the sell button's text with the calculated value.
    /// </summary>
    /// <param name="sellValue">The amount the tower will sell for.</param>
    public void UpdateSellTowerValue(int sellValue)
    {
        if (sellTowerValueText != null) sellTowerValueText.text = "Sell for : " + sellValue;
    }

    /// <summary>
    /// Animates the wave timer panel on or off screen.
    /// </summary>
    /// <param name="enable">True to show the timer, false to hide it.</param>
    public void EnableWaveTimer(bool enable)
    {
        RectTransform rect = waveTimer.GetComponent<RectTransform>();
        float yOffset = enable ? -waveTimerOffset : waveTimerOffset;
        
        Vector3 offset = new Vector3(0, yOffset);

        waveTimerMoveCo = StartCoroutine(animatorUI.ChangePositionCo(rect, offset));
        waveTimerTextBlinkEffect.EnableBlink(enable);
    }
    
    /// <summary>
    /// Instantly resets the wave timer panel to its default position.
    /// Used for scene transitions.
    /// </summary>
    public void SnapTimerToDefaultPosition()
    {
        if (waveTimer == null) return;
        if (waveTimerMoveCo != null) StopCoroutine(waveTimerMoveCo);
        waveTimer.localPosition = waveTimerDefaultPosition;
    }

    /// <summary>
    /// Animates the sell tower panel on or off screen.
    /// </summary>
    /// <param name="enable">True to show the panel, false to hide it.</param>
    /// <param name="sellValue">The sell value to display on the button.</param>
    public void EnableSellTowerUI(bool enable, int sellValue = 0)
    {
        if (sellTowerUI == null) return;

        RectTransform rect = sellTowerUI.GetComponent<RectTransform>();
        float yOffset = enable ? -sellTowerOffset : sellTowerOffset;
        
        Vector3 offset = new Vector3(0, yOffset);

        if (enable && sellTowerValueText != null) sellTowerValueText.text = "Sell for : " + sellValue;

        sellTowerMoveCo = StartCoroutine(animatorUI.ChangePositionCo(rect, offset));
        if (sellTowerTextBlinkEffect != null) sellTowerTextBlinkEffect.EnableBlink(enable);
    }

    /// <summary>
    /// Instantly resets the sell tower panel to its default position.
    /// Used for scene transitions.
    /// </summary>
    public void SnapSellTowerToDefaultPosition()
    {
        if (sellTowerUI == null) return;
        if (sellTowerMoveCo != null) StopCoroutine(sellTowerMoveCo);
        sellTowerUI.localPosition = sellTowerDefaultPosition;
    }

    /// <summary>
    /// Called by the "Force Wave" button.
    /// Tells the WaveManager to start the next wave immediately.
    /// </summary>
    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.StartNewWave();
    }

    /// <summary>
    /// Called by the "Sell Tower" button.
    /// Tells the BuildManager to sell the currently selected tower.
    /// </summary>
    public void SellTowerButton()
    {
        BuildManager buildManager = FindFirstObjectByType<BuildManager>();
        if (buildManager != null) buildManager.SellSelectedTower();
    }
    
    private void FindWaveManager()
    {
        if (waveManager == null || waveManager.gameObject == null) 
            waveManager = FindFirstObjectByType<WaveManager>();
    }

    /// <summary>
    /// Animates the "Next Wave Details" panel on or off screen.
    /// Populates the panel with data from the WaveManager.
    /// </summary>
    /// <param name="enable">True to show the panel, false to hide it.</param>
    public void EnableNextWaveDetails(bool enable)
    {
        if (nextWaveDetailsUI == null) return;

        FindWaveManager();

        if (enable && waveManager != null)
        {
            WaveDetails[] levelWaves = waveManager.GetLevelWaves();
            int currentWaveIndex = waveManager.GetCurrentWaveIndex();

            if (currentWaveIndex < levelWaves.Length)
            {
                WaveDetails nextWave = levelWaves[currentWaveIndex];

                if (waveNumberText != null) waveNumberText.text = $"WAVE {currentWaveIndex + 1}/{levelWaves.Length}";

                if (enemyDetailsText != null)
                {
                    enemyDetailsText.text = BuildEnemyDetailsString(nextWave);
                    
                    // Force the text to update its size
                    Canvas.ForceUpdateCanvases();
                    
                    // Resize parent based on text height
                    RectTransform parentRect = nextWaveDetailsUI.GetComponent<RectTransform>();
                    RectTransform textRect = enemyDetailsText.GetComponent<RectTransform>();
                    
                    float newHeight = textRect.sizeDelta.y + detailsBasePadding;
                    parentRect.sizeDelta = new Vector2(parentRect.sizeDelta.x, newHeight);
                }
            }
        }

        if (nextWaveDetailsMoveCo != null) StopCoroutine(nextWaveDetailsMoveCo);

        RectTransform rect = nextWaveDetailsUI.GetComponent<RectTransform>();
        
        if (enable)
        {
            // When showing, move down by the fixed offset
            float targetY = nextWaveDetailsDefaultPosition.y - nextWaveDetailsOffset;
            Vector3 targetPosition = new Vector3(
                nextWaveDetailsDefaultPosition.x, 
                targetY, 
                nextWaveDetailsDefaultPosition.z
            );
            Vector3 offset = targetPosition - nextWaveDetailsUI.localPosition;
            nextWaveDetailsMoveCo = StartCoroutine(animatorUI.ChangePositionCo(rect, offset));
        }
        else
        {
            // When hiding, calculate how much to move up based on current box height
            float currentHeight = rect.sizeDelta.y;
            // Calculate the difference in height from the original
            float heightDifference = currentHeight - detailsBasePadding;
            // Add extra offset to ensure it goes completely off-screen
            float hideOffset = nextWaveDetailsOffset + heightDifference + 80f; // Added extra 20 pixels
            
            Vector3 offset = new Vector3(0, hideOffset, 0);
            nextWaveDetailsMoveCo = StartCoroutine(animatorUI.ChangePositionCo(rect, offset));
        }
    }

    /// <summary>
    /// Instantly resets the next wave details panel to its default position.
    /// </summary>
    public void SnapNextWaveDetailsToDefaultPosition()
    {
        if (nextWaveDetailsUI == null) return;
        if (nextWaveDetailsMoveCo != null) StopCoroutine(nextWaveDetailsMoveCo);
        nextWaveDetailsUI.localPosition = nextWaveDetailsDefaultPosition;
    }

    /// <summary>
    /// Builds a formatted string of enemy counts for the details panel.
    /// </summary>
    /// <param name="waveDetails">The WaveDetails object for the upcoming wave.</param>
    /// <returns>A formatted, multi-line string.</returns>
    private string BuildEnemyDetailsString(WaveDetails waveDetails)
    {
        System.Collections.Generic.List<string> enemyLines = new System.Collections.Generic.List<string>();
        
        if (waveDetails.basicEnemy > 0) enemyLines.Add($"{waveDetails.basicEnemy} x Basic");
        if (waveDetails.fastEnemy > 0) enemyLines.Add($"{waveDetails.fastEnemy} x Fast");
        if (waveDetails.swarmEnemy > 0) enemyLines.Add($"{waveDetails.swarmEnemy} x Swarm");
        if (waveDetails.heavyEnemy > 0) enemyLines.Add($"{waveDetails.heavyEnemy} x Heavy");
        if (waveDetails.stealthEnemy > 0) enemyLines.Add($"{waveDetails.stealthEnemy} x Stealth");
        if (waveDetails.flyingEnemy > 0) enemyLines.Add($"{waveDetails.flyingEnemy} x Flying");
        if (waveDetails.flyingBossEnemy > 0) enemyLines.Add($"<color=red>{waveDetails.flyingBossEnemy} x Flying Boss</color>");
        if (waveDetails.spiderBossEnemy > 0) enemyLines.Add($"<color=red>{waveDetails.spiderBossEnemy} x Spider Boss</color>");
        
        if (enemyLines.Count == 0) return "No enemies";
        
        return string.Join("\n", enemyLines);
    }
    
    /// <summary>
    /// Resets the dynamic size of the details panel when the UI is disabled.
    /// </summary>
    private void OnDisable()
    {
        if (nextWaveDetailsUI != null)
        {
            RectTransform rect = nextWaveDetailsUI.GetComponent<RectTransform>();
            rect.sizeDelta = nextWaveDetailsOriginalSize;
        }
    }
}