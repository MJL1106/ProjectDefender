using System;
using TMPro;
using UnityEngine;

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

    [Header("Victory and Defeat")] 
    [SerializeField] private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;
    [SerializeField] private GameObject levelCompletedUI;

    private void Awake()
    {
        animatorUI = GetComponentInParent<UIAnimator>();
        ui = GetComponentInParent<UI>();
        
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            pauseUI = canvas.GetComponentInChildren<UIPause>(true);
        }
        
        if (pauseUI == null)
        {
            GameObject pauseUI = GameObject.Find("Pause_UI");
            if (pauseUI != null)
            {
                this.pauseUI = pauseUI.GetComponent<UIPause>();
            }
        }

        if (waveTimer != null) waveTimerDefaultPosition = waveTimer.localPosition;
        if (sellTowerUI != null) sellTowerDefaultPosition = sellTowerUI.localPosition;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10)) ui.SwitchTo(pauseUI.gameObject);
    }

    public void EnableVictoryUI(bool enable)
    {
        if (victoryUI != null) victoryUI.SetActive(enable);
    }

    public void EnableGameOverUI(bool enable)
    {
        if (gameOverUI != null) gameOverUI.SetActive(enable);
    }

    public void EnableLevelCompletedUI(bool enable)
    {
        if (levelCompletedUI != null) levelCompletedUI.SetActive(enable);
    }

    public void ShakeCurrencyUI()
    {
        ui.animUI.Shake(currencyText.transform.parent);
    }

    public void ShakeHealthUI()
    {
        ui.animUI.Shake(healthPointsText.transform.parent);
    }

    public void UpdateHealthPointsUI(int value, int maxValue)
    {
        int newValue = maxValue - value;
        healthPointsText.text = "Threat : " + newValue + "/" + maxValue;
    }
    
    public void UpdateCurrencyUI(int value)
    {
        currencyText.text = "resources : " + value;
    }

    public void UpdateWaveTimerUI(float value)
    {
        waveTimerText.text = "seconds : " + value.ToString("00");
    }
    
    public void UpdateSellTowerValue(int sellValue)
    {
        if (sellTowerValueText != null)
        {
            sellTowerValueText.text = "Sell for : " + sellValue;
        }
    }

    public void EnableWaveTimer(bool enable)
    {
        RectTransform rect = waveTimer.GetComponent<RectTransform>();
        float yOffset = enable ? -waveTimerOffset : waveTimerOffset;
        
        Vector3 offset = new Vector3(0, yOffset);

        waveTimerMoveCo = StartCoroutine(animatorUI.ChangePositionCo(rect, offset));
        waveTimerTextBlinkEffect.EnableBlink(enable);
    }
    
    public void SnapTimerToDefaultPosition()
    {
        if (waveTimer == null) return;

        if (waveTimerMoveCo != null) StopCoroutine(waveTimerMoveCo);

        waveTimer.localPosition = waveTimerDefaultPosition;
    }

    public void EnableSellTowerUI(bool enable, int sellValue = 0)
    {
        if (sellTowerUI == null) return;

        RectTransform rect = sellTowerUI.GetComponent<RectTransform>();
        float yOffset = enable ? -sellTowerOffset : sellTowerOffset;
        
        Vector3 offset = new Vector3(0, yOffset);

        if (enable && sellTowerValueText != null)
        {
            sellTowerValueText.text = "Sell for : " + sellValue;
        }

        sellTowerMoveCo = StartCoroutine(animatorUI.ChangePositionCo(rect, offset));
        if (sellTowerTextBlinkEffect != null) sellTowerTextBlinkEffect.EnableBlink(enable);
    }

    public void SnapSellTowerToDefaultPosition()
    {
        if (sellTowerUI == null) return;

        if (sellTowerMoveCo != null) StopCoroutine(sellTowerMoveCo);

        sellTowerUI.localPosition = sellTowerDefaultPosition;
    }

    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.StartNewWave();
    }

    public void SellTowerButton()
    {
        BuildManager buildManager = FindFirstObjectByType<BuildManager>();
        if (buildManager != null)
        {
            buildManager.SellSelectedTower();
        }
    }
}