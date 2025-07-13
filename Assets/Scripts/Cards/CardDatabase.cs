using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "CardDatabase", menuName = "Cards/CardDatabase")]
public class CardDatabase : ScriptableObject
{
    public static CardDatabase instance;

    private void OnEnable()
    {
        instance = this;
    }
    [System.Serializable]
    public class CardEntry
    {
        public int cardId;
        public GameObject cardPrefab;
    }

    public List<CardEntry> cards = new List<CardEntry>();

    private Dictionary<int, GameObject> _lookup;

    public void Init()
    {
        if (_lookup == null)
        {
            _lookup = new Dictionary<int, GameObject>();
            foreach (var entry in cards)
            {
                if (entry.cardId != 0 && entry.cardPrefab != null)
                    _lookup[entry.cardId] = entry.cardPrefab;
            }
        }
    }


    // Static version: requires a CardDatabase instance as parameter
    public static GameObject GetCard(int cardId)
    {
        if (instance == null) return null;
        instance.Init();
        instance._lookup.TryGetValue(cardId, out var prefab);
        return prefab;
    }

    // Find all cards matching a predicate

    // Static version: requires a CardDatabase instance as parameter
    public static List<GameObject> FindCards(System.Func<CardMaster, bool> predicate)
    {
        var result = new List<GameObject>();
        if (instance == null || instance.cards == null) return result;
        foreach (var entry in instance.cards)
        {
            if (entry.cardPrefab == null) continue;
            var cardMaster = entry.cardPrefab.GetComponent<CardMaster>();
            if (cardMaster != null && predicate(cardMaster))
                result.Add(entry.cardPrefab);
        }
        return result;
    }
}
