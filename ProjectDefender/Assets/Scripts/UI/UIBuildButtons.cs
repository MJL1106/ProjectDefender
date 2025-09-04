using System;
using UnityEngine;

public class UIBuildButtons : MonoBehaviour
{
    private UIAnimator uiAnim;
    private bool isActive;

    [SerializeField]
    private float yPositionOffset;

    private void Awake()
    {
        uiAnim = GetComponentInParent<UIAnimator>();
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.B)) ShowBuildButtons();
    }

    public void ShowBuildButtons()
    {
        isActive = !isActive;

        float yOffset = isActive ? yPositionOffset : -yPositionOffset;
        Vector3 offset = new Vector3(0, yOffset);
        
        uiAnim.ChangePosition(transform, offset);
    }
}
