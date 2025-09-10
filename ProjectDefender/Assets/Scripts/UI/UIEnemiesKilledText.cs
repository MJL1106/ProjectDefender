using System;
using TMPro;
using UnityEngine;

public class UIEnemiesKilledText : MonoBehaviour
{
    private TextMeshProUGUI myText;
    private GameManager gameManager;

    private void Awake()
    {
        myText = GetComponent<TextMeshProUGUI>();
        gameManager = FindFirstObjectByType<GameManager>();
    }

    private void OnEnable()
    {
        myText.text = "Enemies killed: " + gameManager.enemiesKilled;
    }
}
