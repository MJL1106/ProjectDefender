using System;
using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private TileAnimator tileAnimator;
    private UI ui;

    private void Awake()
    {
        tileAnimator = FindFirstObjectByType<TileAnimator>();
        ui = FindFirstObjectByType<UI>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) StartCoroutine(LoadLevelCo());
    }

    private IEnumerator LoadLevelCo()
    {
        tileAnimator.BringUpMainGrid(false);
        ui.EnableMainMenuUI(false);

        yield return tileAnimator.GetActiveCoroutine();
        
        tileAnimator.EnableMainSceneObjects(false);
        
        LoadScene("Level_1");
    }

    private void LoadScene(string sceneNameToLoad) =>
        SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Additive);
}
