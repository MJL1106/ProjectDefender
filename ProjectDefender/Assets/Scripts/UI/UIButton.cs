using UnityEngine;
using UnityEngine.EventSystems;

public class UIButton : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerDownHandler
{
    private UIAnimator uiAnim;
    private RectTransform myRect;

    [SerializeField] private float showcaseScale = 1.1f;
    [SerializeField] private float scaleUpDuration = .25f;

    private Coroutine scaleCoroutine;
    
    private void Awake()
    {
        uiAnim = GetComponentInParent<UIAnimator>();
        myRect = GetComponent<RectTransform>();
    }
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        scaleCoroutine = StartCoroutine(uiAnim.ChangeScaleCo(myRect, showcaseScale, scaleUpDuration));
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (scaleCoroutine != null) StopCoroutine(scaleCoroutine);
        
        scaleCoroutine = StartCoroutine(uiAnim.ChangeScaleCo(myRect, 1, scaleUpDuration));
    }

    public void OnPointerDown(PointerEventData eventData)
    {
        myRect.localScale = new Vector3(1, 1, 1);
    }
}
