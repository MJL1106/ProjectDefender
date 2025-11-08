using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Pool;

/// <summary>
/// A singleton manager for handling object pooling.
/// Creates and manages pools for enemies, projectiles, and VFX to improve performance.
/// </summary>
public class ObjectPoolManager : MonoBehaviour
{
    public static ObjectPoolManager instance;
    
    [Header("Object Pool Details")]
    [SerializeField] private GameObject[] enemyPools;
    [SerializeField] private GameObject[] projectilePools;
    [SerializeField] private GameObject[] vfxPools;
    [SerializeField] private int defaultPoolSize = 50; // Initial number of objects to create for each pool
    [SerializeField] private int maxPoolSize = 500; // Max objects allowed in a pool before errors

    private Dictionary<GameObject, ObjectPool<GameObject>> poolDictionary;

    private void Awake()
    {
        if (instance == null) instance = this;
        else Destroy(gameObject);
    }

    private void Start()
    {
        InitializePools();
    }

    /// <summary>
    /// Retrieves an object from the pool for the specified prefab.
    /// Activates the object and sets its position, rotation, and parent.
    /// </summary>
    /// <param name="prefab">The prefab to get an instance of.</param>
    /// <param name="position">The world position to spawn at.</param>
    /// <param name="rotation">The rotation to apply. Defaults to Quaternion.identity.</param>
    /// <param name="parent">The parent transform to set. Defaults to null.</param>
    public GameObject Get(GameObject prefab, Vector3 position, Quaternion? rotation = null, Transform parent = null)
    {
        if (!poolDictionary.ContainsKey(prefab))
        {
            Debug.LogWarning("No pool was for for game object " + prefab.name + ". Creating new pool!");
            CreateNewPool(prefab);
        }
        
        GameObject objectToGet = poolDictionary[prefab].Get();
        objectToGet.transform.position = position;
        objectToGet.transform.rotation = rotation ?? Quaternion.identity;
        objectToGet.transform.parent = parent;
        objectToGet.SetActive(true);

        return objectToGet;
    }

    /// <summary>
    /// Returns an object to its corresponding pool.
    /// Finds the original prefab via the 'PooledObject' component.
    /// </summary>
    public void Remove(GameObject objectToRemove)
    {
        GameObject originalPrefab = objectToRemove.GetComponent<PooledObject>()?.originalPrefab;

        if (originalPrefab == null)
        {
            Debug.LogWarning("You do not have object pool for this game object. Game object will be destroyed.");
            Destroy(objectToRemove);
            return;
        }
        
        poolDictionary[originalPrefab].Release(objectToRemove);
    }

    /// <summary>
    /// Creates all predefined pools specified in the inspector arrays.
    /// </summary>
    private void InitializePools()
    {
        poolDictionary = new Dictionary<GameObject, ObjectPool<GameObject>>();

        foreach (GameObject prefab in enemyPools)
        {
            CreateNewPool(prefab);
        }
        
        foreach (GameObject prefab in projectilePools)
        {
            CreateNewPool(prefab);
        }
        
        foreach (GameObject prefab in vfxPools)
        {
            CreateNewPool(prefab);
        }
    }
    
    /// <summary>
    /// Creates a new object pool for a given prefab and adds it to the dictionary.
    /// </summary>
    private void CreateNewPool(GameObject prefab)
    {
        var pool = new ObjectPool<GameObject>
            (
                createFunc: () => NewPoolObject(prefab),
                //actionOnGet: obj => obj.SetActive(true),
                actionOnRelease: obj =>
                {
                    obj.SetActive(false);
                    obj.transform.parent = transform;
                },
                actionOnDestroy: obj => Destroy(obj),
                collectionCheck: false,
                defaultCapacity: defaultPoolSize,
                maxSize: maxPoolSize
            );
        
        poolDictionary.Add(prefab, pool);
        StartCoroutine(PreloadPoolCo(pool, defaultPoolSize));
    }

    /// <summary>
    /// Pre-warms a pool by getting and releasing the default number of objects.
    /// Helps prevent frame drops on the first spawn of an object type.
    /// </summary>
    private IEnumerator PreloadPoolCo(ObjectPool<GameObject> poolToPreload, int count)
    {
        List<GameObject> preloadedObjects = new List<GameObject>();
        for (int i = 0; i < count; i++)
        {
            GameObject obj = poolToPreload.Get();
            preloadedObjects.Add(obj);
            obj.SetActive(false);
            yield return null;
        }

        foreach (GameObject obj in preloadedObjects)
        {
            poolToPreload.Release(obj);
        }
    }

    /// <summary>
    /// The 'create' function for the object pool.
    /// Instantiates a new prefab and adds a 'PooledObject' component to track it.
    /// </summary>
    private GameObject NewPoolObject(GameObject prefab)
    {
        GameObject newObject = Instantiate(prefab);
        newObject.AddComponent<PooledObject>().originalPrefab = prefab;
        
        return newObject;
    }
}