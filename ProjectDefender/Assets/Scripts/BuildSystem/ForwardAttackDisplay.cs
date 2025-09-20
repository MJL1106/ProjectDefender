using UnityEngine;

public class ForwardAttackDisplay : MonoBehaviour
{
    [SerializeField] private LineRenderer leftLine;
    [SerializeField] private LineRenderer rightLine;
    [SerializeField] private float attackRange;

    public void CreateLines(bool showLines, float newRange)
    {
        leftLine.enabled = showLines;
        rightLine.enabled = showLines;

        if (!showLines) return;

        attackRange = newRange;
        UpdateLines();
    }

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
