using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

/// <summary>
/// Manages all game settings, including audio sliders and sensitivity.
/// Applies settings on startup and saves them to PlayerPrefs when closed.
/// </summary>
public class UISettings : MonoBehaviour
{
    private CameraController camController;
    
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25; // Multiplier for converting linear slider to logarithmic dB

    [Header("SFX Settings")] 
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private string sfxParameter; // The parameter name in the AudioMixer
    [SerializeField] private TextMeshProUGUI sfxSliderText;
    
    [Header("BGM Settings")] 
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private string bgmParameter; // The parameter name in the AudioMixer
    [SerializeField] private TextMeshProUGUI bgmSliderText;
    
    [Header("Keyboard Sensitivity")] [SerializeField]
    private Slider keyboardSenseSlider;

    [SerializeField] private string keyboardSenseParameter = "keyboardSense";
    [SerializeField] private TextMeshProUGUI keyboardSenseText;

    [SerializeField] private float minKeyboardSens = 60;
    [SerializeField] private float maxKeyboardSens = 240;

    [Header("Mouse Sensitivity")] [SerializeField]
    private Slider mouseSenseSlider;
    [SerializeField] private string mouseSensParameter = "mouseSense";
    [SerializeField] private TextMeshProUGUI mouseSenseText;

    [SerializeField] private float minMouseSense = 1;
    [SerializeField] private float maxMouseSense = 10;

    private void Awake()
    {
        camController = FindFirstObjectByType<CameraController>();
    }
    
    /// <summary>
    /// Loads all settings from PlayerPrefs and applies them.
    /// Called by GameManager on startup.
    /// </summary>
    public void ApplyAllSettingsOnStartup()
    {
        // Load saved values
        float savedSFX = PlayerPrefs.GetFloat(sfxParameter, 0.6f);
        float savedBGM = PlayerPrefs.GetFloat(bgmParameter, 0.6f);
        float savedKeyboard = PlayerPrefs.GetFloat(keyboardSenseParameter, 0.6f);
        float savedMouse = PlayerPrefs.GetFloat(mouseSensParameter, 0.6f);
    
        // Apply audio
        if (audioMixer != null)
        {
            float sfxDB = Mathf.Log10(savedSFX) * mixerMultiplier;
            float bgmDB = Mathf.Log10(savedBGM) * mixerMultiplier;
        
            audioMixer.SetFloat(sfxParameter, sfxDB);
            audioMixer.SetFloat(bgmParameter, bgmDB);
        }
    
        // Apply camera sensitivity
        if (camController != null)
        {
            float keyboardSens = Mathf.Lerp(minKeyboardSens, maxKeyboardSens, savedKeyboard);
            float mouseSens = Mathf.Lerp(minMouseSense, maxMouseSense, savedMouse);
        
            camController.AdjustKeyboardSensitivity(keyboardSens);
            camController.AdjustMouseSensitivity(mouseSens);
        }
    }

    /// <summary>
    /// Called by the SFX slider. Updates the AudioMixer and text.
    /// </summary>
    /// <param name="value">The slider value (0.0 to 1.0).</param>
    public void SFXSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(sfxParameter, newValue);
        
        sfxSliderText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    /// <summary>
    /// Called by the BGM slider. Updates the AudioMixer and text.
    /// </summary>
    /// <param name="value">The slider value (0.0 to 1.0).</param>
    public void BGMSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(bgmParameter, newValue);
        
        bgmSliderText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    /// <summary>
    /// Called by the keyboard sensitivity slider. Updates the CameraController and text.
    /// </summary>
    /// <param name="value">The slider value (0.0 to 1.0).</param>
    public void KeyboardSensitivity(float value)
    {
        float newSensitivity = Mathf.Lerp(minKeyboardSens, maxKeyboardSens, value);
        camController.AdjustKeyboardSensitivity(newSensitivity);

        keyboardSenseText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    /// <summary>
    /// Called by the mouse sensitivity slider. Updates the CameraController and text.
    /// </summary>
    /// <param name="value">The slider value (0.0 to 1.0).</param>
    public void MouseSensitivity(float value)
    {
        float newSensitivity = Mathf.Lerp(minMouseSense, maxMouseSense, value);
        camController.AdjustMouseSensitivity(newSensitivity);
        
        mouseSenseText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    /// <summary>
    /// Saves all slider values to PlayerPrefs when the settings panel is closed.
    /// </summary>
    private void OnDisable()
    {
        PlayerPrefs.SetFloat(keyboardSenseParameter, keyboardSenseSlider.value);
        PlayerPrefs.SetFloat(mouseSensParameter, mouseSenseSlider.value);
        PlayerPrefs.SetFloat(bgmParameter, bgmSlider.value);
        PlayerPrefs.SetFloat(sfxParameter, sfxSlider.value);
    }

    /// <summary>
    /// Loads saved values from PlayerPrefs and sets the sliders' positions.
    /// </summary>
    private void OnEnable()
    {
        keyboardSenseSlider.value = PlayerPrefs.GetFloat(keyboardSenseParameter, .6f);
        mouseSenseSlider.value = PlayerPrefs.GetFloat(mouseSensParameter, .6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParameter, .6f);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParameter, .6f);
    }
}