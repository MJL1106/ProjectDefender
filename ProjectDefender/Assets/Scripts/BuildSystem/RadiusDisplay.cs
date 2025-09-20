using System;
using UnityEngine;

[RequireComponent(typeof(LineRenderer))]

public class RadiusDisplay : MonoBehaviour
{
    private LineRenderer lineRenderer;

    [SerializeField] private float lineWidth = .1f;
    [SerializeField] private float radius;
    private int segments = 50;
    
    private void Awake()
    {
        lineRenderer = GetComponent<LineRenderer>();
        lineRenderer.positionCount = segments + 1; // Add extra points to close the circle
        lineRenderer.useWorldSpace = true;
        lineRenderer.startWidth = lineWidth;
        lineRenderer.endWidth = lineWidth;
        lineRenderer.material = FindFirstObjectByType<BuildManager>().GetAttackRadiusMat();
    }
    
    public void CreateCircle(bool showCircle, float radius = 0)
    {
        lineRenderer.enabled = showCircle;
        
        if (showCircle == false) return;
        
        float angle = 0;
        Vector3 centre = transform.position;

        for (int i = 0; i < segments; i++)
        {
            float x = Mathf.Sin(Mathf.Deg2Rad * angle) * radius;
            float z = Mathf.Cos(Mathf.Deg2Rad * angle) * radius;
            
            lineRenderer.SetPosition(i, new Vector3(x + centre.x, centre.y, z + centre.z));
            angle += 360f / segments;
        }
        
        lineRenderer.SetPosition(segments, lineRenderer.GetPosition(0));
    }
}
