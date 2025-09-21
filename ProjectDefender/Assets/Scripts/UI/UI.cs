using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.Serialization;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Image fadeImageUI;
    [SerializeField] private GameObject[] elementsUI;

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

        if (GameManager.instance.IsTestingLevel())
        {
            SwitchTo(inGameUI.gameObject);
        }
    }

    public void SwitchTo(GameObject uiToEnable)
    {
        foreach (GameObject ui in elementsUI)
        {
            ui.SetActive(false);
        }
    
        if (uiToEnable != null)
            uiToEnable.SetActive(true);
    }

    public void EnableMainMenuUI(bool enable)
    {
        if (enable)
        {
            SwitchTo(mainMenuUI.gameObject);
        }
        else
        {
            SwitchTo(null);
        }
    }

    public void EnableInGameUI(bool enable)
    {
        if (enable)
        {
            SwitchTo(inGameUI.gameObject);
        }
        else
        {
            inGameUI.SnapTimerToDefaultPosition();
            SwitchTo(null);
        }
    }

    public void QuitButton()
    {
        if (!fadeImageUI.gameObject.activeSelf) return;
        
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        else Application.Quit();
    }

    public void ActivateFadeEffect(bool fadeIn)
    {
        if (fadeIn) animUI.ChangeColour(fadeImageUI, 0, 1.5f);
        else animUI.ChangeColour(fadeImageUI, 1, 1.5f);

    }
}
