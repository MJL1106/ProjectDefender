using System;
using UnityEngine;

/// <summary>
/// Draws a circular radius visualization for tower attack ranges.
/// Uses LineRenderer to create a segmented circle at ground level.
/// </summary>
[RequireComponent(typeof(LineRenderer))]
public class RadiusDisplay : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [SerializeField] private float lineWidth = 0.1f;
    [SerializeField] private float radius;
    
    private int segments = 50; // Number of line segments to form the circle
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1; // Extra point to close the circle
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = FindFirstObjectByType<BuildManager>().GetAttackRadiusMat();
        lineRenderer.shadowCastingMode = UnityEngine.Rendering.ShadowCastingMode.Off;
    }
    
    /// <summary>
    /// Toggles circle visibility and sets radius size.
    /// </summary>
    /// <param name="showCircle">Whether to display the radius circle</param>
    /// <param name="radius">Radius size in world units</param>
    public void CreateCircle(bool showCircle, float radius = 0)
    {
        lineRenderer.enabled = showCircle;
        if (showCircle == false) return;
        
        float angle = 0;
        Vector3 centre = transform.position;

        // Generate circle points using trigonometry
        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            
            lineRenderer.SetPosition(i, new Vector3(x + centre.x, centre.y, z + centre.z));
            angle += 360f / segments;
        }
        
        // Close the circle by connecting last point to first
        lineRenderer.SetPosition(segments, lineRenderer.GetPosition(0));
    }
}