using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UISettings : MonoBehaviour
{
    private CameraController camController;
    
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
    }

    private void OnEnable()
    {
        keyboardSenseSlider.value = PlayerPrefs.GetFloat(keyboardSenseParameter, .6f);
        mouseSenseSlider.value = PlayerPrefs.GetFloat(mouseSensParameter, .6f);
    }
}
