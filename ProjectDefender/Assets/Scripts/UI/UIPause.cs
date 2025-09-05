using System;
using UnityEngine;

public class UIPause : MonoBehaviour
{
    private UI ui;
    private UIGame inGameUI;

    [SerializeField] private GameObject[] pauseUiElements;

    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        inGameUI = ui.GetComponentInChildren<UIGame>(true);
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.F10)) ui.SwitchTo(inGameUI.gameObject);
    }

    private void OnEnable()
    {
        Time.timeScale = 0;
    }

    private void OnDisable()
    {
        Time.timeScale = 1;
    }

    public void SwitchPauseUIElemens(GameObject elementToEnable)
    {
        foreach (GameObject obj in pauseUiElements)
        {
            obj.SetActive(false);
        }
        
        elementToEnable.SetActive(true);
    }
}
