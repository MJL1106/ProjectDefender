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
    
    private void Awake()
    {
        BuildButtonsHolderUI = GetComponentInChildren<UIBuildButtonsHolder>(true);
        settingsUI = GetComponentInChildren<UISettings>(true);
        mainMenuUI = GetComponentInChildren<UIMainMenu>(true);
        inGameUI = GetComponentInChildren<UIGame>(true);
        animUI = GetComponent<UIAnimator>();

        //ActivateFadeEffect(true);
        
        SwitchTo(settingsUI.gameObject);
        //SwitchTo(uiMainMenu.gameObject);
        SwitchTo(inGameUI.gameObject);
    }

    public void SwitchTo(GameObject uiToEnable)
    {
        foreach (GameObject ui in elementsUI)
        {
            ui.SetActive(false);
        }

        uiToEnable.SetActive(true);
    }

    public void QuitButton()
    {
        if (EditorApplication.isPlaying) EditorApplication.isPlaying = false;
        else Application.Quit();
    }

    public void ActivateFadeEffect(bool fadeIn)
    {
        if (fadeIn) animUI.ChangeColour(fadeImageUI, 0, 2);
        else animUI.ChangeColour(fadeImageUI, 1, 2);

    }
}
