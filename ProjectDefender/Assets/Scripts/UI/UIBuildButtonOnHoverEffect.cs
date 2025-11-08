using System;
using UnityEngine;
using UnityEngine.EventSystems;

/// <summary>
/// A visual-only component that moves a build button up/down on hover/selection.
/// </summary>
public class UIBuildButtonOnHoverEffect : MonoBehaviour
{
    [SerializeField] private float adjustmentSpeed = 10;

    [SerializeField] private float showcaseY; // The target Y position when selected
    [SerializeField] private float defaultY; // The resting Y position
    
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


    /// <summary>
    /// Enables or disables the hover movement and resets the button's position.
    /// </summary>
    /// <param name="buttonsMenuActive">True if the build menu is active, false otherwise.</param>
    public void ToggleMovement(bool buttonsMenuActive)
    {
        canMove = buttonsMenuActive;
        SetTargetY(defaultY);

        if (buttonsMenuActive == false) SetPositionToDefault();
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

    /// <summary>
    /// Sets the target Y position to the 'showcase' or 'default' state.
    /// </summary>
    /// <param name="showcase">True to move up, false to move to default.</param>
    public void ShowCaseButton(bool showcase)
    {
        if (showcase) SetTargetY(showcaseY);
        else SetTargetY(defaultY);      
    }
}