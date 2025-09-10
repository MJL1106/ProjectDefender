using System;
using TMPro;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    private CameraController camController;
    
    [SerializeField] private AudioMixer audioMixer;
    [SerializeField] private float mixerMultiplier = 25;

    [Header("SFX Settings")] 
    [SerializeField] private Slider sfxSlider;
    [SerializeField] private string sfxParameter;
    [SerializeField] private TextMeshProUGUI sfxSliderText;
    
    [Header("BGM Settings")] 
    [SerializeField] private Slider bgmSlider;
    [SerializeField] private string bgmParameter;
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

    public void SFXSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(sfxParameter, newValue);
        
        sfxSliderText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void BGMSliderValue(float value)
    {
        float newValue = MathF.Log10(value) * mixerMultiplier;
        audioMixer.SetFloat(bgmParameter, newValue);
        
        bgmSliderText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void KeyboardSensitivity(float value)
    {
        float newSensitivity = Mathf.Lerp(minKeyboardSens, maxKeyboardSens, value);
        camController.AdjustKeyboardSensitivity(newSensitivity);

        keyboardSenseText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    public void MouseSensitivity(float value)
    {
        float newSensitivity = Mathf.Lerp(minMouseSense, maxMouseSense, value);
        camController.AdjustMouseSensitivity(newSensitivity);
        
        mouseSenseText.text = Mathf.RoundToInt(value * 100) + "%";
    }

    private void OnDisable()
    {
        PlayerPrefs.SetFloat(keyboardSenseParameter, keyboardSenseSlider.value);
        PlayerPrefs.SetFloat(mouseSensParameter, mouseSenseSlider.value);
        PlayerPrefs.SetFloat(bgmParameter, bgmSlider.value);
        PlayerPrefs.SetFloat(sfxParameter, sfxSlider.value);
    }

    private void OnEnable()
    {
        keyboardSenseSlider.value = PlayerPrefs.GetFloat(keyboardSenseParameter, .6f);
        mouseSenseSlider.value = PlayerPrefs.GetFloat(mouseSensParameter, .6f);
        bgmSlider.value = PlayerPrefs.GetFloat(bgmParameter, .6f);
        sfxSlider.value = PlayerPrefs.GetFloat(sfxParameter, .6f);
    }
}
