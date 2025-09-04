using System;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

public class UI : MonoBehaviour
{
    [SerializeField] private Image uiFadeImage;
    [SerializeField] private GameObject[] uiElements;

    private UIAnimator uiAnim;
    private UISettings uiSettings;
    private UIMainMenu uiMainMenu;
    private UIGame uiInGame;
    
    private void Awake()
    {
        uiSettings = GetComponentInChildren<UISettings>(true);
        uiMainMenu = GetComponentInChildren<UIMainMenu>(true);
        uiInGame = GetComponentInChildren<UIGame>(true);
        uiAnim = GetComponent<UIAnimator>();

        //ActivateFadeEffect(true);
        
        SwitchTo(uiSettings.gameObject);
        SwitchTo(uiMainMenu.gameObject);
        //SwitchTo(uiInGame.gameObject);
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

    public void ActivateFadeEffect(bool fadeIn)
    {
        if (fadeIn) uiAnim.ChangeColour(uiFadeImage, 0, 2);
        else uiAnim.ChangeColour(uiFadeImage, 1, 2);

    }
}
