using System;
using System.Collections;
using UnityEngine;

/// <summary>
/// A utility component attached to pooled objects (like VFX).
/// Automatically returns the object to the ObjectPoolManager after a set delay.
/// </summary>
public class SelfRemoveToPool : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    private ParticleSystem particle;
    
    [SerializeField] private float removeDelay = 1; // Time in seconds before returning to pool


    private void Awake()
    {
        objectPool = ObjectPoolManager.instance;
        particle = GetComponentInChildren<ParticleSystem>();
    }
    
    /// <summary>
    /// When enabled (retrieved from pool), starts the removal coroutine.
    /// Also restarts any attached particle system.
    /// </summary>
    private void OnEnable()
    {
        if (particle != null)
        {
            particle.Clear();
            particle.Play();
        }
        
        StartCoroutine(RemoveWithDelayCo());
    }

    /// <summary>
    /// Waits for the 'removeDelay' then tells the ObjectPoolManager to remove this object.
    /// </summary>
    private IEnumerator RemoveWithDelayCo()
    {
        yield return new WaitForSeconds(removeDelay);
        
        objectPool.Remove(gameObject);
    }
}