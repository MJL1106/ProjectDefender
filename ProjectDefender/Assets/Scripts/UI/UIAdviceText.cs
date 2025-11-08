using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

/// <summary>
/// Displays a random advice string from a list when enabled.
/// </summary>
public class UIAdviceText : MonoBehaviour
{
    private TextMeshProUGUI myText;

    [SerializeField] private string[] advices; // List of advice strings to choose from

    private void OnEnable()
    {
        if (myText == null) myText = GetComponent<TextMeshProUGUI>();

        int randomIndex = Random.Range(0, advices.Length);
        myText.text = advices[randomIndex];
    }
}