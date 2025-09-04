using TMPro;
using UnityEngine;

public class UIGame : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI healthPointsText;
    [SerializeField] private TextMeshProUGUI currencyText;

    public void UpdateHealthPointsUI(int value, int maxValue)
    {
        int newValue = maxValue - value;
        healthPointsText.text = "Threat : " + newValue + "/" + maxValue;
    }
    
    public void UpdateCurrencyUI(int value)
    {
        currencyText.text = "resources : " + value;
    }
}
