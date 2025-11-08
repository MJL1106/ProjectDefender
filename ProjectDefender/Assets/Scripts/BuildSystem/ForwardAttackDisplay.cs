using System;
using UnityEngine;

/// <summary>
/// Displays attack range visualization for forward-facing towers.
/// Creates two parallel lines showing the left and right boundaries of the attack cone.
/// </summary>
public class ForwardAttackDisplay : MonoBehaviour
{
    [SerializeField] private LineRenderer leftLine;
    [SerializeField] private LineRenderer rightLine;
    [SerializeField] private float attackRange;

    private void Awake()
    {
        // Disable shadows for UI visualization elements
        leftLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
        rightLine.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }

    /// <summary>
    /// Toggles line visibility and updates attack range visualization.
    /// </summary>
    /// <param name="showLines">Whether to show the attack range lines</param>
    /// <param name="newRange">The attack range distance to visualize</param>
    public void CreateLines(bool showLines, float newRange)
    {
        leftLine.enabled = showLines;
        rightLine.enabled = showLines;

        if (!showLines) return;

        attackRange = newRange;
        UpdateLines();
    }

    /// <summary>
    /// Refreshes line positions based on current transform.
    /// Called when tower rotates or attack range changes.
    /// </summary>
    public void UpdateLines()
    {
        DrawLine(leftLine);
        DrawLine(rightLine);
    }

    private void DrawLine(LineRenderer line)
    {
        Vector3 start = line.transform.position;
        Vector3 end = start + (transform.forward * attackRange);

        line.SetPosition(0, start);
        line.SetPosition(1, end);
    }
}