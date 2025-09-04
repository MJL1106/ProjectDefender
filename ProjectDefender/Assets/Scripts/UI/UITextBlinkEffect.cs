using System;
using TMPro;
using UnityEngine;
using UnityEngine.Serialization;

public class UITextBlinkEffect : MonoBehaviour
{
    private TextMeshProUGUI myText;

    [SerializeField] private float changeValueSpeed;
    private float targetAlpha;
    private bool canBlink;

    private void Awake()
    {
        myText = GetComponent<TextMeshProUGUI>();
    }

    private void Update()
    {
        if (canBlink == false) return;
        
        if (Mathf.Abs(myText.color.a - targetAlpha) > .01f)
        {
            float newAlpha = Mathf.Lerp(myText.color.a, targetAlpha, changeValueSpeed * Time.deltaTime);
            ChangeColourAlpha(newAlpha);
        }
        else
        {
            ChangeTargetAlpha();
        }
    }

    public void EnableBlink(bool enable)
    {
        canBlink = enable;

        if (canBlink == false) ChangeColourAlpha(1);
    }

    private void ChangeTargetAlpha() => targetAlpha = Mathf.Approximately(targetAlpha, 1) ? 0 : 1;
    
    private void ChangeColourAlpha(float newAlpha)
    {
        Color myColor = myText.color;
        myText.color = new Color(myColor.r, myColor.g, myColor.b, newAlpha);
    }
}
