using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameManager : MonoBehaviour
{
    [SerializeField] private int currency;
    
    [SerializeField] private int maxHp;
    [SerializeField] private int currentHp;

    private UIGame inGameUI;
    private WaveManager currentActiveWaveManager;
    private LevelManager levelManager;

    private void Awake()
    {
        inGameUI = FindFirstObjectByType<UIGame>(FindObjectsInactive.Include);
        levelManager = FindFirstObjectByType<LevelManager>();
    }

    private void Start()
    {
        currentHp = maxHp;
        inGameUI.UpdateHealthPointsUI(currentHp,maxHp);
        inGameUI.UpdateCurrencyUI(currency);
    }

    public void LevelCompleted()
    {
        string currentLevelName = levelManager.currentLevelName;
        int nextLevelIndex = SceneUtility.GetBuildIndexByScenePath(currentLevelName) + 1;

        if (nextLevelIndex >= SceneManager.sceneCountInBuildSettings)
        {
            inGameUI.EnableVictoryUI(true);
        }
        else
        {
            levelManager.LoadLevel("Level_" + nextLevelIndex);
        }
    }

    public void UpdateGameManager(int levelCurrency, WaveManager newWaveManager)
    {
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
    }

    public void UpdateCurrency(int value)
    {
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
