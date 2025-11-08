using System;
using UnityEngine;

/// <summary>
/// Manages the pause menu.
/// Pauses the game (Time.timeScale = 0) when enabled.
/// </summary>
public class UIPause : MonoBehaviour
{
    private UI ui;
    private UIGame inGameUI;

    [SerializeField] private GameObject[] pauseUiElements; // Child panels (e.g., main pause, settings)

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        inGameUI = ui.GetComponentInChildren<UIGame>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10)) ui.SwitchTo(inGameUI.gameObject);
    }

    /// <summary>
    /// Pauses the game when the pause menu is activated.
    /// </summary>
    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    /// <summary>
    /// Unpauses the game when the pause menu is deactivated.
    /// </summary>
    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    /// <summary>
    /// Switches between child panels within the pause menu (e.g., to open settings).
    /// </summary>
    /// <param name="elementToEnable">The child panel to activate.</param>
    public void SwitchPauseUIElemens(GameObject elementToEnable)
    {
        foreach (GameObject obj in pauseUiElements)
        {
            obj.SetActive(false);
        }
        
        elementToEnable.SetActive(true);
    }
}