using System;
using System.Collections.Generic;
using NUnit.Framework;
using UnityEngine;

/// <summary>
/// Manages the visual "chain" for the Harpoon projectile.
/// Instantiates, activates, and positions a series of link prefabs.
/// Also handles the "electrify" VFX when the harpoon is active.
/// </summary>
public class HarpoonVisuals : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    
    [SerializeField] private Transform startPoint; // The base of the chain (on the tower)
    [SerializeField] private Transform endPoint; // The tip of the chain (on the projectile)
    [Space]
    [SerializeField] private GameObject linkPrefab;
    [SerializeField] private Transform linksParent;
    [SerializeField] private float linkDistance = .2f; // Distance between each chain link
    [SerializeField] private int maxLinks = 100;

    private List<ProjectileHarpoonLink> links = new List<ProjectileHarpoonLink>();

    [Space] 
    [SerializeField] private GameObject onElectrifyVfx; // VFX that plays on the enemy
    [SerializeField] private Vector3 vfxOffset;
    private GameObject currentVfx;

    private void Start()
    {
        InitializeLinks();
        objectPool = ObjectPoolManager.instance;
    }

    private void Update()
    {
        if (endPoint == null) return;
        
        ActivateLinksIfNeeded();
    }

    /// <summary>
    /// Creates the electrification VFX on the target.
    /// </summary>
    /// <param name="targetTransform">The transform to parent the VFX to (the enemy).</param>
    public void CreateElectrifyVFX(Transform targetTransform)
    {
        currentVfx = objectPool.Get(onElectrifyVfx, targetTransform.position + vfxOffset, Quaternion.identity, targetTransform);
    }

    /// <summary>
    /// Removes the electrification VFX.
    /// </summary>
    public void DestroyElectrifyVFX()
    {
        if (currentVfx != null) objectPool.Remove(currentVfx);
    }

    /// <summary>
    /// Toggles the chain visuals on or off.
    /// When enabling, sets a new endpoint. When disabling, resets the endpoint to the start.
    /// </summary>
    /// <param name="enable">True to show the chain, false to hide it.</param>
    /// <param name="newEndPoint">The target transform (projectile) for the chain to follow.</param>
    public void EnableChainVisuals(bool enable, Transform newEndPoint = null)
    {
        if (enable) endPoint = newEndPoint;

        if (enable == false)
        {
            endPoint = startPoint;
            DestroyElectrifyVFX();
        }
    }
    
    /// <summary>
    /// Calculates the required number of links and activates/positions them.
    /// </summary>
    private void ActivateLinksIfNeeded()
    {
        Vector3 direction = (endPoint.position - startPoint.position).normalized;
        float distance = Vector3.Distance(startPoint.position, endPoint.position);
        int activeLinksAmount = Mathf.Min(maxLinks, Mathf.CeilToInt(distance / linkDistance));

        for (int i = 0; i < links.Count; i++)
        {
            if (i < activeLinksAmount)
            {
                Vector3 newPosition = startPoint.position + direction * linkDistance * (i + 1);
                links[i].EnableLink(true, newPosition);
            }
            else
            {
                links[i].EnableLink(false, Vector3.zero);
            }

            if (i != links.Count - 1) links[i].UpdateLineRenderer(links[i], links[i + 1]);
        }
    }

    /// <summary>
    /// Pre-instantiates the maximum number of chain links.
    /// </summary>
    private void InitializeLinks()
    {
        for (int i = 0; i < maxLinks; i++)
        {
            ProjectileHarpoonLink newLink =
                Instantiate(linkPrefab, startPoint.position, Quaternion.identity, linksParent)
                    .GetComponent<ProjectileHarpoonLink>();

            links.Add(newLink);
        }
    }
}