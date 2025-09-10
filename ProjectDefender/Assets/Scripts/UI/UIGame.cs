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

    [Header("Victory and Defeat")] [SerializeField]
    private GameObject victoryUI;
    [SerializeField] private GameObject gameOverUI;

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

    public void EnableWaveTimer(bool enable)
    {
        if (!gameObject.activeInHierarchy) return;
        
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

    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.StartNewWave();
    }
}
