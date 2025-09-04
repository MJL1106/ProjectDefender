using System;
using UnityEditor;
using UnityEngine;

public class UI : MonoBehaviour
{
    [SerializeField] private GameObject[] uiElements;

    private UISettings uiSettings;
    private UIMainMenu uiMainMenu;
    private UIGame uiInGame;
    
    private void Awake()
    {
        uiSettings = GetComponentInChildren<UISettings>(true);
        uiMainMenu = GetComponentInChildren<UIMainMenu>(true);
        uiInGame = GetComponentInChildren<UIGame>(true);
        
        SwitchTo(uiSettings.gameObject);
       // SwitchTo(uiMainMenu.gameObject);
        SwitchTo(uiInGame.gameObject);
    }

    public void SwitchTo(GameObject uiToEnable)
    {
        foreach (GameObject ui in uiElements)
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
}
