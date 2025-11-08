using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

/// <summary>
/// Manages all top-level UI panels (Menu, Game, Settings) and scene fade transitions.
/// Acts as the central hub for accessing UI components.
/// </summary>
public class UI : MonoBehaviour
{
    [SerializeField] private Image fadeImageUI;
    [SerializeField] private GameObject[] elementsUI; // All top-level UI panels (main menu, in-game, settings, etc.)

    private UISettings settingsUI;
    private UIMainMenu mainMenuUI;
    
    public UIGame inGameUI { get; private set; }
    public UIAnimator animUI { get; private set; }
    public UIBuildButtonsHolder BuildButtonsHolderUI { get; private set; }

    [Header("UI SFX")] 
    public AudioSource onHoverSFX;
    public AudioSource onClickSFX;
    
    private void Awake()
    {
        BuildButtonsHolderUI = GetComponentInChildren<UIBuildButtonsHolder>(true);
        settingsUI = GetComponentInChildren<UISettings>(true);
        mainMenuUI = GetComponentInChildren<UIMainMenu>(true);
        inGameUI = GetComponentInChildren<UIGame>(true);
        animUI = GetComponent<UIAnimator>();

        ActivateFadeEffect(true);
        
        SwitchTo(settingsUI.gameObject);
        SwitchTo(mainMenuUI.gameObject);

        if (GameManager.instance.IsTestingLevel()) SwitchTo(inGameUI.gameObject);
    }

    /// <summary>
    /// Disables all UI panels and enables the specified one.
    /// </summary>
    /// <param name="uiToEnable">The specific UI panel (e.g., main menu) to activate. Pass null to hide all.</param>
    public void SwitchTo(GameObject uiToEnable)
    {
        foreach (GameObject ui in elementsUI)
        {
            ui.SetActive(false);
        }
    
        if (uiToEnable != null) uiToEnable.SetActive(true);
    }

    /// <summary>
    /// Helper function to switch to the main menu or hide all UI.
    /// </summary>
    /// <param name="enable">True to show main menu, false to hide all panels.</param>
    public void EnableMainMenuUI(bool enable)
    {
        if (enable) SwitchTo(mainMenuUI.gameObject);
        else SwitchTo(null);
    }

    /// <summary>
    /// Helper function to switch to the in-game UI or hide all UI.
    /// </summary>
    /// <param name="enable">True to show in-game UI, false to hide all panels.</param>
    public void EnableInGameUI(bool enable)
    {
        if (enable) SwitchTo(inGameUI.gameObject);
        else
        {
            inGameUI.SnapTimerToDefaultPosition();
            inGameUI.SnapSellTowerToDefaultPosition();
            SwitchTo(null);
        }
    }

    /// <summary>
    /// Quits the application or stops play mode in the editor.
    /// </summary>
    public void QuitButton()
    {
        #if UNITY_EDITOR
                UnityEditor.EditorApplication.isPlaying = false;
        #else
                Application.Quit();
        #endif
    }

    /// <summary>
    /// Triggers the full-screen fade image to fade in (to black) or out (to clear).
    /// </summary>
    /// <param name="fadeIn">True to fade out (to clear), false to fade in (to black).</param>
    public void ActivateFadeEffect(bool fadeIn)
    {
        if (fadeIn) animUI.ChangeColour(fadeImageUI, 0, 1.5f);
        else animUI.ChangeColour(fadeImageUI, 1, 1.5f);
    }
}