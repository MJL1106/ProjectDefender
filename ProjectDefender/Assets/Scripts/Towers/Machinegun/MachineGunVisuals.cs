using System.Collections;
using UnityEngine;

public class MachineGunVisuals : MonoBehaviour
{
    [Header("Recoil details")] [SerializeField]
    private float recoilOffset = -.2f;
    [SerializeField] private float recoverSpeed = .25f;
    [SerializeField] private ParticleSystem onAttackFx;

    public void RecoilVfx(Transform gunPoint)
    {
        PlayOnAttackFx(gunPoint.position);
        StartCoroutine(RecoilCo(gunPoint));
    }

    private void PlayOnAttackFx(Vector3 position)
    {
        onAttackFx.transform.position = position;
        onAttackFx.Play();
    }

    private IEnumerator RecoilCo(Transform gunPoint)
    {
        Transform objectToMove = gunPoint.transform.parent;
        Vector3 originalPosition = objectToMove.localPosition;
        Vector3 recoildPosition = originalPosition + new Vector3(0, 0, recoilOffset);

        objectToMove.localPosition = recoildPosition;

        while (objectToMove.localPosition != originalPosition)
        {
            objectToMove.localPosition = Vector3.MoveTowards(objectToMove.localPosition, originalPosition,
                recoverSpeed * Time.deltaTime);

            yield return null;
        }
    }
}
