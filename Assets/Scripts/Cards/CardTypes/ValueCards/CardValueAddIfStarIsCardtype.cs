using System.Collections.Generic;
using UnityEngine;

public class CardValueAddIfStarIsCardtype : CardMaster
{
    [Header("CardType Filter (if empty, match empty slots)")]
    public List<CardType> cardTypes = new List<CardType>();
    public List<CardRarity> cardRarities = new List<CardRarity>();
    public List<CardBond> cardBonds = new List<CardBond>();

    [Header("Card Value Settings")]
    public float addDamage = 0f;
    public float addHealth = 0f;
    public float addProbability = 0f;
    public float addAmount = 0f;
    public float addMana = 0f;
    public float addCoin = 0f;

    public override void OnCardEnable()
    {
        if (BoardArea.instance != null)
        {
            GetStarCards(out List<CardMaster> starCards, out List<UnityEngine.Vector2Int> emptySlots, out List<UnityEngine.Vector2Int> lockedSlots);
            int matchCount = 0;
            if ((cardTypes == null || cardTypes.Count == 0) && (cardRarities == null || cardRarities.Count == 0) && (cardBonds == null || cardBonds.Count == 0))
            {
                // If no filter, match empty slots
                matchCount = emptySlots != null ? emptySlots.Count : 0;
            }
            else if (starCards != null)
            {
                foreach (var card in starCards)
                {
                    if (card != null)
                    {
                        bool matched = false;
                        // CardType match
                        if (cardTypes != null && cardTypes.Count > 0)
                        {
                            foreach (var ct in cardTypes)
                            {
                                if (card.card_type == ct)
                                {
                                    matched = true;
                                    break;
                                }
                            }
                        }
                        // CardRarity match
                        if (!matched && cardRarities != null && cardRarities.Count > 0)
                        {
                            foreach (var cr in cardRarities)
                            {
                                if (card.card_rarity == cr)
                                {
                                    matched = true;
                                    break;
                                }
                            }
                        }
                        // CardBond match
                        if (!matched && cardBonds != null && cardBonds.Count > 0 && card.card_bonds != null)
                        {
                            foreach (var cb in cardBonds)
                            {
                                if (card.card_bonds.Contains(cb))
                                {
                                    matched = true;
                                    break;
                                }
                            }
                        }
                        if (matched)
                        {
                            matchCount++;
                        }
                    }
                }
            }
            if (matchCount > 0)
            {
                var valuePairs = new (NumberType, float)[] {
                    (NumberType.Damage, addDamage),
                    (NumberType.Health, addHealth),
                    (NumberType.Probability, addProbability),
                    (NumberType.Amount, addAmount),
                    (NumberType.Mana, addMana),
                    (NumberType.Coin, addCoin)
                };
                foreach (var (nType, nValue) in valuePairs)
                {
                    if (Mathf.Abs(nValue) > 0.0001f)
                    {
                        UpdateNumberValue(nType, nValue * matchCount, this);
                    }
                }
            }
        }
        base.OnCardEnable();
    }

    public override string GetDescription()
    {
        List<string> parts = new List<string>();
        if (addDamage != 0) parts.Add($"Damage: {addDamage}");
        if (addHealth != 0) parts.Add($"Health: {addHealth}");
        if (addProbability != 0) parts.Add($"Probability: {addProbability}");
        if (addAmount != 0) parts.Add($"Amount: {addAmount}");
        if (addMana != 0) parts.Add($"Mana: {addMana}");
        if (addCoin != 0) parts.Add($"Coin: {addCoin}");
        if (parts.Count == 0) return "No permanent stat increases.";
        string joined = string.Join(", ", parts);
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), joined));
    }
}
