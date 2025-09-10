using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UI ui;
    private UIAnimator uiAnim;
    private RectTransform myRect;

    [SerializeField] private float showcaseScale = 1.1f;
    [SerializeField] private float scaleUpDuration = .25f;

    private Coroutine scaleCoroutine;
    [Space] [SerializeField] private UITextBlinkEffect myTextBlinkEffect;
    
    private void Awake()
    {
        ui = GetComponentInParent<UI>();
        uiAnim = GetComponentInParent<UIAnimator>();
        myRect = GetComponent<RectTransform>();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        AudioManager.instance?.PlaySFX(ui.onHoverSFX);
        
        scaleCoroutine = StartCoroutine(uiAnim.ChangeScaleCo(myRect, showcaseScale, scaleUpDuration));

        if (myTextBlinkEffect != null) myTextBlinkEffect.EnableBlink(false);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        scaleCoroutine = StartCoroutine(uiAnim.ChangeScaleCo(myRect, 1, scaleUpDuration));
        if (myTextBlinkEffect != null) myTextBlinkEffect.EnableBlink(true);
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        AudioManager.instance?.PlaySFX(ui.onClickSFX);
        myRect.localScale = new Vector3(1, 1, 1);
    }
}
