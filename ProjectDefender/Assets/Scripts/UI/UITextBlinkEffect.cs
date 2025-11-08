using TMPro;
using UnityEngine;

/// <summary>
/// Creates a blinking effect on a TextMeshProUGUI component by lerping its alpha.
/// </summary>
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

    /// <summary>
    /// Toggles the blink effect on or off.
    /// </summary>
    /// <param name="enable">True to start blinking, false to stop and set alpha to 1.</param>
    public void EnableBlink(bool enable)
    {
        canBlink = enable;

        if (canBlink == false) ChangeColourAlpha(1);
    }

    /// <summary>
    /// Flips the target alpha between 0 and 1.
    /// </summary>
    private void ChangeTargetAlpha() => targetAlpha = Mathf.Approximately(targetAlpha, 1) ? 0 : 1;
    
    /// <summary>
    /// Sets the alpha of the text's color.
    /// </summary>
    /// <param name="newAlpha">The new alpha value (0-1).</param>
    private void ChangeColourAlpha(float newAlpha)
    {
        Color myColor = myText.color;
        myText.color = new Color(myColor.r, myColor.g, myColor.b, newAlpha);
    }
}