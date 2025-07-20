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
    public static List<GameObject> FindCards(System.Func<CardMaster, bool> predicate, bool includeInternalCards = false)
    {
        var result = new List<GameObject>();
        if (instance == null || instance.cards == null || instance.cards.Count == 0) return result;
        foreach (var entry in instance.cards)
        {
            if (entry.cardPrefab == null) continue;
            var cardMaster = entry.cardPrefab.GetComponent<CardMaster>();
            if (cardMaster != null && predicate(cardMaster))
                if (includeInternalCards) { result.Add(entry.cardPrefab); }
                else
                {
                    // Gun, Base, Internal cards are not for public random card generator
                    if (cardMaster.card_type != CardMaster.CardType.Base && cardMaster.card_type != CardMaster.CardType.Gun && cardMaster.card_type != CardMaster.CardType.Internal)
                        result.Add(entry.cardPrefab);
                }
        }
        return Shuffle(result);
    }

    public static GameObject GetRandomCard(System.Func<CardMaster, bool> predicate,  bool includeInternalCards = false)
    {
        if (instance == null || instance.cards == null || instance.cards.Count == 0) return null;
        List<GameObject> filteredCards = FindCards(predicate, includeInternalCards);
        if (filteredCards.Count == 0) return null;
        return filteredCards[Random.Range(0, filteredCards.Count)];
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
