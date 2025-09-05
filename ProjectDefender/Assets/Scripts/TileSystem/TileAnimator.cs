using System.Collections;
using UnityEngine;
using UnityEngine.Serialization;

public class TileAnimator : MonoBehaviour
{
    [SerializeField] private float yMovementDuration = .1f;

    [Header("Build Slot Movement")] [SerializeField]
    private float buildSlotYOffset = 0.25f;
    
    public void MoveTile(Transform objectToMove, Vector3 targetPosition)
    {
        StartCoroutine(MoveTileCo(objectToMove, targetPosition));
    }

    public IEnumerator MoveTileCo(Transform objectToMove, Vector3 targetPosition)
    {
        float time = 0;
        Vector3 startPosition = objectToMove.position;

        while (time < yMovementDuration)
        {
            objectToMove.position = Vector3.Lerp(startPosition, targetPosition, time / yMovementDuration);

            time += Time.deltaTime;
            yield return null;
        }

        objectToMove.position = targetPosition;
    }

    public float GetBuildOffset() => buildSlotYOffset;
    public float GetTravelDuration() => yMovementDuration;
}
