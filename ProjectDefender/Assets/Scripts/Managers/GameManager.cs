using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    public static GameManager instance;
    
    private UIGame inGameUI;
    private WaveManager currentActiveWaveManager;
    private LevelManager levelManager;
    private CameraEffects cameraEffects;
    
    [SerializeField] private int currency;
    [SerializeField] private int maxHp;
    [SerializeField] private int currentHp;
    
    public int enemiesKilled { get; private set; }

    private bool gameLost;

    private void Awake()
    {
        instance = this;
        
        inGameUI = FindFirstObjectByType<UIGame>(FindObjectsInactive.Include);
        levelManager = FindFirstObjectByType<LevelManager>();
        cameraEffects = FindFirstObjectByType<CameraEffects>();
    }

    private void Start()
    {
        currentHp = maxHp;

        if (IsTestingLevel())
        {
            currency += 9999;
            currentHp += 9999;
        }
        
        inGameUI.UpdateHealthPointsUI(currentHp,maxHp);
        inGameUI.UpdateCurrencyUI(currency);
    }

    public bool IsTestingLevel() => levelManager == null;

    public IEnumerator LevelFailedCo()
    {
        gameLost = true;
        currentActiveWaveManager.DeactivateWaveManager();
        cameraEffects.FocusOnCastle();

        yield return cameraEffects.GetActiveCameraCo();
        
        inGameUI.EnableGameOverUI(true);
    }

    public void LevelCompleted() => StartCoroutine(LevelCompletedCo());

    public IEnumerator LevelCompletedCo()
    {
        cameraEffects.FocusOnCastle();

        yield return cameraEffects.GetActiveCameraCo();

        if (levelManager.HasNoMoreLevels())
        { 
            inGameUI.EnableVictoryUI(true);
        }
        else
        {
            inGameUI.EnableLevelCompletedUI(true);
            PlayerPrefs.SetInt(levelManager.GetNextLevelName() + " unlocked", 1);
        }
    }

    public void UpdateGameManager(int levelCurrency, WaveManager newWaveManager)
    {
        gameLost = false;
        enemiesKilled = 0;
        
        currentActiveWaveManager = newWaveManager;
        currency = levelCurrency;
        currentHp = maxHp;
        
        inGameUI.UpdateHealthPointsUI(currentHp, maxHp);
        inGameUI.UpdateCurrencyUI(currency);
    }

    public void UpdateHp(int value)
    {
        currentHp += value;
        inGameUI.UpdateHealthPointsUI(currentHp,maxHp);
        inGameUI.ShakeHealthUI();
        
        if (currentHp <= 0 && gameLost == false) StartCoroutine(LevelFailedCo());
    }

    public void UpdateCurrency(int value)
    {
        enemiesKilled++;
        currency += value;
        inGameUI.UpdateCurrencyUI(currency);
    }

    public bool HasEnoughCurrency(int price)
    {
        if (price < currency)
        {
            currency = currency - price;
            inGameUI.UpdateCurrencyUI(currency);
            return true;
        }

        return false;
    }
}
