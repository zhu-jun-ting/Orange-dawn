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
        [System.NonSerialized]
        public CardMaster cardMaster; // cached reference
    }


    public List<CardEntry> cards = new List<CardEntry>();

    // --- Caching for performance ---
    private Dictionary<CardMaster.CardRarity, List<GameObject>> _cardsByRarity;
    private Dictionary<CardMaster.CardType, List<GameObject>> _cardsByType;
    private bool _cacheInitialized = false;

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
        InitCardMasterCache();
        InitFilterCaches();
    }

    // Cache CardMaster reference in CardEntry to avoid GetComponent in loops
    private void InitCardMasterCache()
    {
        foreach (var entry in cards)
        {
            if (entry.cardPrefab != null && entry.cardMaster == null)
                entry.cardMaster = entry.cardPrefab.GetComponent<CardMaster>();
        }
    }

    // Cache cards by rarity and type for common queries
    private void InitFilterCaches()
    {
        if (_cacheInitialized) return;
        _cardsByRarity = new Dictionary<CardMaster.CardRarity, List<GameObject>>();
        _cardsByType = new Dictionary<CardMaster.CardType, List<GameObject>>();
        foreach (var entry in cards)
        {
            var cm = entry.cardMaster;
            if (entry.cardPrefab == null || cm == null) continue;
            // By rarity
            if (!_cardsByRarity.ContainsKey(cm.card_rarity))
                _cardsByRarity[cm.card_rarity] = new List<GameObject>();
            _cardsByRarity[cm.card_rarity].Add(entry.cardPrefab);
            // By type
            if (!_cardsByType.ContainsKey(cm.card_type))
                _cardsByType[cm.card_type] = new List<GameObject>();
            _cardsByType[cm.card_type].Add(entry.cardPrefab);
        }
        _cacheInitialized = true;
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
        instance.Init();

        // Fast path: if predicate is a simple rarity or type filter, use cache
        if (predicate != null)
        {
            // Try to detect simple rarity filter
            var method = predicate.Method;
            if (method.Name == "<OnRoomLoaded>b__" || method.Name.Contains("card_rarity"))
            {
                // Try to extract rarity from closure (best effort)
                foreach (CardMaster.CardRarity rarity in System.Enum.GetValues(typeof(CardMaster.CardRarity)))
                {
                    bool isSimple = true;
                    // Test if predicate returns true for a dummy CardMaster of this rarity
                    // (Not perfect, but works for lambdas like card => card.card_rarity == X)
                    foreach (var entry in instance.cards)
                    {
                        var cm = entry.cardMaster;
                        if (cm != null && cm.card_rarity == rarity && predicate(cm))
                        {
                            // Use cache for this rarity
                            foreach (var go in instance._cardsByRarity[rarity])
                            {
                                var cm2 = go.GetComponent<CardMaster>();
                                if (cm2 != null && predicate(cm2))
                                    if (includeInternalCards || (cm2.card_type != CardMaster.CardType.Base && cm2.card_type != CardMaster.CardType.Gun && !cm2.isInternal))
                                        result.Add(go);
                            }
                            return result;
                        }
                    }
                }
            }
            // Try to detect simple type filter
            if (method.Name.Contains("card_type"))
            {
                foreach (CardMaster.CardType type in System.Enum.GetValues(typeof(CardMaster.CardType)))
                {
                    foreach (var entry in instance.cards)
                    {
                        var cm = entry.cardMaster;
                        if (cm != null && cm.card_type == type && predicate(cm))
                        {
                            foreach (var go in instance._cardsByType[type])
                            {
                                var cm2 = go.GetComponent<CardMaster>();
                                if (cm2 != null && predicate(cm2))
                                    if (includeInternalCards || (cm2.card_type != CardMaster.CardType.Base && cm2.card_type != CardMaster.CardType.Gun && !cm2.isInternal))
                                        result.Add(go);
                            }
                            return result;
                        }
                    }
                }
            }
        }

        // Fallback: full scan
        foreach (var entry in instance.cards)
        {
            var cardMaster = entry.cardMaster;
            if (entry.cardPrefab == null || cardMaster == null) continue;
            if (predicate(cardMaster))
            {
                if (includeInternalCards) { result.Add(entry.cardPrefab); }
                else
                {
                    if (cardMaster.card_type != CardMaster.CardType.Base && cardMaster.card_type != CardMaster.CardType.Gun && !cardMaster.isInternal)
                        result.Add(entry.cardPrefab);
                }
            }
        }
        return result;
    }

    public static GameObject GetRandomCard(System.Func<CardMaster, bool> predicate, bool includeInternalCards = false)
    {
        if (instance == null || instance.cards == null || instance.cards.Count == 0) return null;
        instance.Init();
        // Fast path: if predicate is a simple rarity or type filter, use cache
        if (predicate != null)
        {
            var method = predicate.Method;
            if (method.Name == "<OnRoomLoaded>b__" || method.Name.Contains("card_rarity"))
            {
                foreach (CardMaster.CardRarity rarity in System.Enum.GetValues(typeof(CardMaster.CardRarity)))
                {
                    foreach (var entry in instance.cards)
                    {
                        var cm = entry.cardMaster;
                        if (cm != null && cm.card_rarity == rarity && predicate(cm))
                        {
                            var list = instance._cardsByRarity[rarity];
                            if (list != null && list.Count > 0)
                            {
                                // Filter for internal/excluded cards
                                var filtered = includeInternalCards ? list : list.FindAll(go => {
                                    var cm2 = go.GetComponent<CardMaster>();
                                    return cm2 != null && cm2.card_type != CardMaster.CardType.Base && cm2.card_type != CardMaster.CardType.Gun && !cm2.isInternal;
                                });
                                if (filtered.Count == 0) return null;
                                // Pick a random one that matches the predicate
                                var candidates = filtered.FindAll(go => {
                                    var cm2 = go.GetComponent<CardMaster>();
                                    return cm2 != null && predicate(cm2);
                                });
                                if (candidates.Count == 0) return null;
                                return candidates[Random.Range(0, candidates.Count)];
                            }
                        }
                    }
                }
            }
            if (method.Name.Contains("card_type"))
            {
                foreach (CardMaster.CardType type in System.Enum.GetValues(typeof(CardMaster.CardType)))
                {
                    foreach (var entry in instance.cards)
                    {
                        var cm = entry.cardMaster;
                        if (cm != null && cm.card_type == type && predicate(cm))
                        {
                            var list = instance._cardsByType[type];
                            if (list != null && list.Count > 0)
                            {
                                var filtered = includeInternalCards ? list : list.FindAll(go => {
                                    var cm2 = go.GetComponent<CardMaster>();
                                    return cm2 != null && cm2.card_type != CardMaster.CardType.Base && cm2.card_type != CardMaster.CardType.Gun && !cm2.isInternal;
                                });
                                if (filtered.Count == 0) return null;
                                var candidates = filtered.FindAll(go => {
                                    var cm2 = go.GetComponent<CardMaster>();
                                    return cm2 != null && predicate(cm2);
                                });
                                if (candidates.Count == 0) return null;
                                return candidates[Random.Range(0, candidates.Count)];
                            }
                        }
                    }
                }
            }
        }
        // Fallback: full scan
        List<GameObject> filteredCards = FindCards(predicate, includeInternalCards);
        if (filteredCards.Count == 0) return null;
        return filteredCards[Random.Range(0, filteredCards.Count)];
    }
}
