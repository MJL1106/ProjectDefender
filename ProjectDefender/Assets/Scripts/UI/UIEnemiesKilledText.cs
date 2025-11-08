using System;
using TMPro;
using UnityEngine;

/// <summary>
/// Updates a TextMeshProUGUI component to show the total enemies killed.
/// Retrieves the count from the GameManager.
/// </summary>
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