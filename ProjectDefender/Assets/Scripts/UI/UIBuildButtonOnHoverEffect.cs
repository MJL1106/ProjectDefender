using System;
using UnityEngine;
using UnityEngine.EventSystems;

public class UIBuildButtonOnHoverEffect : MonoBehaviour, IPointerExitHandler
{
    [SerializeField] private float adjustmentSpeed = 10;

    [SerializeField] private float showcaseY;
    [SerializeField] private float defaultY;
    [SerializeField] private float selectedY;
    
    private float targetY;

    private bool canMove;
    
    private void Update()
    {
        if (Mathf.Abs(transform.position.y - targetY) > .01f && canMove)
        {
            float newPositionY = Mathf.Lerp(transform.position.y, targetY, adjustmentSpeed * Time.deltaTime);

            SetPositionToActive(newPositionY);
        }
    }


    public void ToggleMovement(bool buttonsMenuActive)
    {
        canMove = buttonsMenuActive;
        SetTargetY(defaultY);

        if (buttonsMenuActive == false)
        {
            SetPositionToDefault();
        }
    }
    
    private void SetPositionToActive(float newPositionY)
    {
        transform.position = new Vector3(transform.position.x, newPositionY, transform.position.z);
    }

    private void SetPositionToDefault()
    {
        transform.position = new Vector3(transform.position.x, defaultY, transform.position.z);
    }

    private void SetTargetY(float newY) => targetY = newY;

    public void ShowCaseButton(bool showcase)
    {
        if (showcase)
        {
            SetTargetY(showcaseY);
        }
        else
        {
            SetTargetY(defaultY);      
        }
    }
    public void OnPointerExit(PointerEventData eventData)
    {
        SetTargetY(selectedY);
    }
}
