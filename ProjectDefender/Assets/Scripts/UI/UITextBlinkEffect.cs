using System;
using TMPro;
using UnityEngine;

public class UITextBlinkEffect : MonoBehaviour
{
    private TextMeshProUGUI myText;

    private void Awake()
    {
        myText = GetComponent<TextMeshProUGUI>();
    }
}
