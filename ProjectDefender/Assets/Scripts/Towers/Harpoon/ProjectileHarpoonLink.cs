using System;
using UnityEngine;

/// <summary>
/// A single link in the harpoon's visual chain.
/// Manages its own mesh, particle VFX, and a LineRenderer connection to the next link.
/// </summary>
public class ProjectileHarpoonLink : MonoBehaviour
{
    private LineRenderer lr;
    private MeshRenderer mesh;
    private ParticleSystem vfx;

    private void Awake()
    {
        lr = GetComponent<LineRenderer>();
        lr.positionCount = 2;

        mesh = GetComponentInChildren<MeshRenderer>();
        vfx = GetComponentInChildren<ParticleSystem>();
        
        EnableLink(false, transform.position);
        EnableVFX(false);
    }

    /// <summary>
    /// Shows or hides the link's mesh and sets its position.
    /// </summary>
    /// <param name="enable">True to show the mesh, false to hide.</param>
    /// <param name="newPosition">The new world-space position for this link.</param>
    public void EnableLink(bool enable, Vector3 newPosition)
    {
        mesh.enabled = enable;
        transform.position = newPosition;
    }

    /// <summary>
    /// Updates the LineRenderer to connect this link to the next one.
    /// </summary>
    /// <param name="startPoint">This link.</param>
    /// <param name="endPoint">The next link in the chain.</param>
    public void UpdateLineRenderer(ProjectileHarpoonLink startPoint, ProjectileHarpoonLink endPoint)
    {
        lr.enabled = startPoint.CurrentlyActive() && endPoint.CurrentlyActive();
        EnableVFX(lr.enabled);

        if (lr.enabled == false) return;

        lr.SetPosition(0, startPoint.transform.position);
        lr.SetPosition(1, endPoint.transform.position);
    }

    /// <summary>
    /// Toggles the link's particle VFX.
    /// </summary>
    private void EnableVFX(bool enable)
    {
        if (enable && vfx.isPlaying == false) vfx.Play();
        else vfx.Stop();
    }

    /// <summary>
    /// Checks if the link's mesh is currently enabled.
    /// </summary>
    public bool CurrentlyActive() => mesh.enabled;
}