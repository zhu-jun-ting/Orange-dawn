using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "ItemDatabase", menuName = "Items/ItemDatabase")]
public class ItemDatabase : ScriptableObject
{
    public static ItemDatabase instance;

    private void OnEnable()
    {
        instance = this;
    }

    [System.Serializable]
    public class ItemEntry
    {
        public int itemId;
        public GameObject itemPrefab;
    }

    public List<ItemEntry> items = new List<ItemEntry>();
    private Dictionary<int, GameObject> _lookup;

    public void Init()
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<int, GameObject>();
            foreach (var entry in items)
            {
                if (entry.itemId != 0 && entry.itemPrefab != null)
                    _lookup[entry.itemId] = entry.itemPrefab;
            }
        }
    }

    public static GameObject GetItem(int itemId)
    {
        if (instance == null) return null;
        instance.Init();
        instance._lookup.TryGetValue(itemId, out var prefab);
        return prefab;
    }

    public static List<GameObject> FindItems(System.Func<ItemMaster, bool> predicate)
    {
        var result = new List<GameObject>();
        if (instance == null || instance.items == null || instance.items.Count == 0) return result;
        foreach (var entry in instance.items)
        {
            if (entry.itemPrefab == null) continue;
            var itemMaster = entry.itemPrefab.GetComponent<ItemMaster>();
            if (itemMaster != null && predicate(itemMaster))
                result.Add(entry.itemPrefab);
        }
        return Shuffle(result);
    }

    public static GameObject GetRandomItem(System.Func<ItemMaster, bool> predicate)
    {
        if (instance == null || instance.items == null || instance.items.Count == 0) return null;
        List<GameObject> filteredItems = FindItems(predicate);
        if (filteredItems.Count == 0) return null;
        return filteredItems[Random.Range(0, filteredItems.Count)];
    }

    public static List<T> Shuffle<T>(List<T> list)
    {
        var rng = new System.Random();
        var shuffled = new List<T>(list);
        int n = shuffled.Count;
        while (n > 1)
        {
            n--;
            int k = rng.Next(n + 1);
            T value = shuffled[k];
            shuffled[k] = shuffled[n];
            shuffled[n] = value;
        }
        return shuffled;
    }
}
