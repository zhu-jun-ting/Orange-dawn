using System.Collections.Generic;
using UnityEngine;

public class ObjectPool : MonoBehaviour
{
    public static ObjectPool Instance;
    private void Awake() { Instance = this; }

    // Each prefab has its own queue and max size
    private Dictionary<GameObject, Queue<GameObject>> pools = new Dictionary<GameObject, Queue<GameObject>>();
    private Dictionary<GameObject, int> maxSizes = new Dictionary<GameObject, int>();
    private int defaultMaxSize = 1000;

    // Set max size for a specific prefab
    public void SetMaxSize(GameObject prefab, int maxSize)
    {
        maxSizes[prefab] = maxSize;
    }

    public GameObject GetObject(GameObject prefab)
    {
        if (pools.ContainsKey(prefab))
        {
            var tempPool = pools[prefab];
            int initialCount = tempPool.Count;
            for (int i = 0; i < initialCount; i++)
            {
                GameObject obj = tempPool.Dequeue();
                if (obj != null)
                    tempPool.Enqueue(obj);
            }
        }
        if (prefab == null)
        {
            Debug.LogError("ObjectPool: prefab is null");
            return null;
        }
        if (!pools.ContainsKey(prefab))
        {
            pools[prefab] = new Queue<GameObject>();
            maxSizes[prefab] = defaultMaxSize;
        }
        var pool = pools[prefab];
        int maxSize = maxSizes[prefab];
        if (pool.Count < maxSize)
        {
            GameObject obj = Instantiate(prefab);
            pool.Enqueue(obj);
            obj.SetActive(true);
            return obj;
        }
        else
        {
            GameObject obj = pool.Dequeue();
            pool.Enqueue(obj);
            obj.SetActive(true);
            return obj;
        }
    }
}
