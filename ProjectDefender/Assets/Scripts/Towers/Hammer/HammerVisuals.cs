using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// Manages all visual animations and particle effects for the TowerHammer.
/// Handles the hammer slam, reload, and associated component animations (valve, wires).
/// </summary>
public class HammerVisuals : MonoBehaviour
{
    private TowerHammer myTower;
    
    [SerializeField] private ParticleSystem[] vfx;
    [SerializeField] private RotateObject valveRotation; // Controls the spinning valve on top
    
    [Header("Hammer Details")] [SerializeField]
    private Transform hammer; // The hammer head
    [SerializeField] private Transform hammerHolder; // The piston/holder for the hammer

    [Space] 
    [SerializeField] private Transform sideWire;
    [SerializeField] private Transform sideHandle;


    [Header("Attack and Release Details")] 
    [SerializeField] private float attackOffsetY;
    [SerializeField] private float attackDuration;
    [SerializeField] private float reloadDuration;

    private void Awake()
    {
        myTower = GetComponent<TowerHammer>();
        reloadDuration = myTower.GetAttackCooldown() - attackDuration;
    }

    /// <summary>
    /// Triggers the full hammer slam and reload animation sequence.
    /// </summary>
    public void HammerAttackAnimation()
    {
        StopAllCoroutines();
        StartCoroutine(HammerAttackCo());
    }

    /// <summary>
    /// Plays the ground-slam particle VFX.
    /// </summary>
    public void PlayAttackAnimation()
    {
        foreach (var p in vfx)
        {
            p.Play();
        }
    }

    /// <summary>
    /// Coroutine that animates the hammer slam down, plays VFX,
    /// then animates the reload back up over the attack cooldown.
    /// </summary>
    private IEnumerator HammerAttackCo()
    {
        valveRotation.AdjustRotationSpeed(25);
        
        StartCoroutine(ChangePositionCo(hammer, -attackOffsetY, attackDuration));
        StartCoroutine(ChangeScaleCo(hammerHolder, 7, attackDuration));

        StartCoroutine(ChangePositionCo(sideHandle, .45f, attackDuration));
        StartCoroutine(ChangeScaleCo(sideWire, .1f, attackDuration));

        yield return new WaitForSeconds(attackDuration);
        PlayAttackAnimation();

        valveRotation.AdjustRotationSpeed(3);
        
        StartCoroutine(ChangePositionCo(hammer, attackOffsetY, reloadDuration));
        StartCoroutine(ChangeScaleCo(hammerHolder, 1, reloadDuration));
        
        StartCoroutine(ChangePositionCo(sideHandle, -.45f, reloadDuration));
        StartCoroutine(ChangeScaleCo(sideWire, 1, reloadDuration));
    }
    
    
    /// <summary>
    /// Coroutine to lerp the hammer's local position by a Y-axis offset.
    /// </summary>
    /// <param name="transform">The transform to move.</param>
    /// <param name="yOffset">The amount to add to the transform's local Y position.</param>
    /// <param name="duration">The time in seconds for the lerp.</param>
    public IEnumerator ChangePositionCo(Transform transform, float yOffset, float duration = .1f)
    {
        float time = 0;

        Vector3 initialPosition = transform.localPosition;
        Vector3 targetPosition = new Vector3(initialPosition.x, initialPosition.y + yOffset, initialPosition.z);

        while (time < duration)
        {
            transform.localPosition = Vector3.Lerp(initialPosition, targetPosition, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localPosition = targetPosition;

    }

    /// <summary>
    /// Coroutine to lerp the hammer's local Y-scale.
    /// </summary>
    /// <param name="transform">The transform to scale.</param>
    /// <param name="newScale">The target local Y-scale. (X and Z scale remain 1).</param>
    /// <param name="duration">The time in seconds for the lerp.</param>
    private IEnumerator ChangeScaleCo(Transform transform, float newScale, float duration = .25f)
    {
        float time = 0;

        Vector3 initialScale = transform.localScale;
        Vector3 targetScale = new Vector3(1,newScale,1);

        while (time < duration)
        {
            transform.localScale = Vector3.Lerp(initialScale, targetScale, time / duration);

            time += Time.deltaTime;
            yield return null;
        }

        transform.localScale = targetScale;
    }
}