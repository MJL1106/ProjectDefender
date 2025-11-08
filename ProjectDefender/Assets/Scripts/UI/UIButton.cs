using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A generic UI button that plays sounds and scales on hover.
/// </summary>
public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private UIAnimator uiAnim;
    private RectTransform myRect;

    [SerializeField] private float showcaseScale = 1.1f; // The scale to pop to on hover
    [SerializeField] private float scaleUpDuration = .25f;

    private Coroutine scaleCoroutine;
    [Space] [SerializeField] private UITextBlinkEffect myTextBlinkEffect;
    
    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        uiAnim = GetComponentInParent<UIAnimator>();
        myRect = GetComponent<RectTransform>();
    }

    /// <summary>
    /// Plays hover sound and scales up.
    /// </summary>
    public virtual void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        AudioManager.instance?.PlaySFX(ui.onHoverSFX);
        
        scaleCoroutine = StartCoroutine(uiAnim.ChangeScaleCo(myRect, showcaseScale, scaleUpDuration));

        if (myTextBlinkEffect != null) myTextBlinkEffect.EnableBlink(false);
    }

    /// <summary>
    /// Scales back to default.
    /// </summary>
    public virtual void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        scaleCoroutine = StartCoroutine(uiAnim.ChangeScaleCo(myRect, 1, scaleUpDuration));
        if (myTextBlinkEffect != null) myTextBlinkEffect.EnableBlink(true);
    }

    /// <summary>
    /// Plays click sound and resets scale.
    /// </summary>
    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.instance?.PlaySFX(ui.onClickSFX);
        myRect.localScale = new Vector3(1, 1, 1);
    }
}