using System;
using TMPro;
using UnityEngine;

public class UIGame : MonoBehaviour
{
    private UIAnimator uiAnimator;
    
    [SerializeField] private TextMeshProUGUI healthPointsText;
    [SerializeField] private TextMeshProUGUI currencyText;
    [Space]
    [SerializeField] private TextMeshProUGUI waveTimerText;
    [SerializeField] private float waveTimerOffset;
    [SerializeField] private UITextBlinkEffect waveTimerTextBlinkEffect;

    private void Awake()
    {
        uiAnimator = GetComponentInParent<UIAnimator>();
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
        
        uiAnimator.ChangePosition(waveTimerTransform, offset);
        waveTimerTextBlinkEffect.EnableBlink(enable);

        //waveTimerText.transform.parent.gameObject.SetActive(enable);
    }

    public void ForceWaveButton()
    {
        WaveManager waveManager = FindFirstObjectByType<WaveManager>();
        waveManager.ForceNextWave();
    }
}
