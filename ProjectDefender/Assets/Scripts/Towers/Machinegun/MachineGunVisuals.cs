using System.Collections;
using UnityEngine;

/// <summary>
/// Manages recoil animation and muzzle flash for the machine gun tower.
/// </summary>
public class MachineGunVisuals : MonoBehaviour
{
    [Header("Recoil details")] 
    [SerializeField] private float recoilOffset = -.2f; // How far back the gun recoils (local z-axis)
    [SerializeField] private float recoverSpeed = .25f; // How quickly the gun returns to its original position
    [SerializeField] private ParticleSystem onAttackFx;

    /// <summary>
    /// Triggers the muzzle flash and recoil animation.
    /// </summary>
    /// <param name="gunPoint">The gun barrel transform. Its parent will be moved for the recoil.</param>
    public void RecoilVfx(Transform gunPoint)
    {
        PlayOnAttackFx(gunPoint.position);
        StartCoroutine(RecoilCo(gunPoint));
    }

    /// <summary>
    /// Plays the muzzle flash particle effect at a position.
    /// </summary>
    /// <param name="position">The world position to spawn the VFX.</param>
    private void PlayOnAttackFx(Vector3 position)
    {
        onAttackFx.transform.position = position;
        onAttackFx.Play();
    }

    /// <summary>
    /// Coroutine to move the gun parent back and then smoothly lerp it forward.
    /// </summary>
    /// <param name="gunPoint">The gun barrel. Its parent is the object that recoils.</param>
    private IEnumerator RecoilCo(Transform gunPoint)
    {
        Transform objectToMove = gunPoint.transform.parent;
        Vector3 originalPosition = objectToMove.localPosition;
        Vector3 recoiledPosition = originalPosition + new Vector3(0, 0, recoilOffset);

        objectToMove.localPosition = recoiledPosition;

        while (objectToMove.localPosition != originalPosition)
        {
            objectToMove.localPosition = Vector3.MoveTowards(objectToMove.localPosition, originalPosition,
                recoverSpeed * Time.deltaTime);

            yield return null;
        }
    }
}