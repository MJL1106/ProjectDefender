using System;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LevelManager : MonoBehaviour
{
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.J)) LoadScene("Level_1");
    }

    private void LoadScene(string sceneNameToLoad) =>
        SceneManager.LoadSceneAsync(sceneNameToLoad, LoadSceneMode.Additive);
}
