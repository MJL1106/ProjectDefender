using System;
using TMPro;
using UnityEngine;
using Random = UnityEngine.Random;

public class UIAdviceText : MonoBehaviour
{
    private TextMeshProUGUI myText;

    [SerializeField] private string[] advices;

    private void OnEnable()
    {
        if (myText == null) myText = GetComponent<TextMeshProUGUI>();

        int randomIndex = Random.Range(0, advices.Length);
        myText.text = advices[randomIndex];
    }
}
