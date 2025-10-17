using System;
using UnityEngine;

public class EnemySpiderEMP : MonoBehaviour
{
    private ObjectPoolManager objectPool;
    
    [SerializeField] private GameObject empVfx;
    [SerializeField] private float moveSpeed = 1;
    [SerializeField] private float empRadius = 2;
    [SerializeField] private float empEffectDuration = 2;
    [SerializeField] private float minLifetime = 0.5f;

    private Vector3 destination;
    private Vector3 originalScale;
    private float shrinkSpeed = 3;
    private float spawnTime;
    private bool shouldShrink;

    private void Awake()
    {
        objectPool = ObjectPoolManager.instance;
        originalScale = transform.localScale;
    }
    
    private void OnEnable()
    {
        shouldShrink = false;
        spawnTime = Time.time;
        transform.localScale = originalScale;
    }

    private void Update()
    {
        MoveTowardsTarget();
        
        if (shouldShrink) Shrink();
    }

    private void Shrink()
    {
        transform.localScale -= Vector3.one * shrinkSpeed * Time.deltaTime;
        
        if (transform.localScale.x <= .01f)
        {
            objectPool.Remove(gameObject);
        }
    }

    private void MoveTowardsTarget()
    {
        float distance = Vector3.Distance(transform.position, destination);
        
        transform.position = Vector3.MoveTowards(transform.position, destination, moveSpeed * Time.deltaTime);

        float timeAlive = Time.time - spawnTime;
        
        if (distance < 0.1f && timeAlive > minLifetime)
        {
            DeactivateEMP();
        }
    }

    public void SetupEMP(float duration, Vector3 newTarget, float empDuration)
    {
        empEffectDuration = duration;
        destination = newTarget;
        shouldShrink = false;
        //Invoke(nameof(DeactivateEMP), empDuration);
    }

    private void DeactivateEMP()
    {
        shouldShrink = true;
    }

    private void OnTriggerEnter(Collider other)
    {
        Tower tower = other.GetComponent<Tower>();
        
        if (tower != null)
        {
            tower.DeactivateTower(empEffectDuration, empVfx);
        }
    }
    
    private void OnDrawGizmos()
    {
        Gizmos.DrawWireSphere(transform.position, empRadius);
    }
}
