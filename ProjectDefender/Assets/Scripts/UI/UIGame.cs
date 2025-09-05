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

    private void Awake()
    {
        animatorUI = GetComponentInParent<UIAnimator>();
        if (animatorUI == null)
            Debug.LogError("UIAnimator not found in parent objects of " + gameObject.name);

        ui = GetComponentInParent<UI>();
        if (ui == null)
            Debug.LogError("UI component not found in parent objects of " + gameObject.name);

        // Use the Canvas search method (this seems to be working based on your debug output)
        Canvas canvas = GetComponentInParent<Canvas>();
        if (canvas != null)
        {
            pauseUI = canvas.GetComponentInChildren<UIPause>(true);
        }
        
        // Fallback to GameObject.Find if canvas method fails
        if (pauseUI == null)
        {
            GameObject pauseUI = GameObject.Find("Pause_UI");
            if (pauseUI != null)
            {
                this.pauseUI = pauseUI.GetComponent<UIPause>();
            }
        }
        
        if (pauseUI == null)
            Debug.LogError("UIPause component not found!");
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10)) ui.SwitchTo(pauseUI.gameObject);
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
        Transform waveTimerTransform = waveTimerText.transform.parent;
        
        float yOffset = enable ? -waveTimerOffset : waveTimerOffset;
        Vector3 offset = new Vector3(0, yOffset);
        
        animatorUI.ChangePosition(waveTimerTransform, offset);
        waveTimerTextBlinkEffect.EnableBlink(enable);

        //waveTimerText.transform.parent.gameObject.SetActive(enable);
    }

    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.ForceNextWave();
    }
}
