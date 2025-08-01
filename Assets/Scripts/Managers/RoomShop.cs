using UnityEngine;
using System.Collections.Generic;

public class RoomShop : RoomGrid
{
    [Header("Shop Type")]
    [Tooltip("Type of shop: Normal, Elite, or Boss \n Normal shops have basic cards, Elite shops have stronger cards, and Condition shops have cards with different conditions.")]
    public ShopType shopType; // Normal, Elite, or Boss
    [Header("Card Count")]
    [Range(1, 4)] public int cardCount = 3; // Number of cards to show in the shop
    public enum ShopType
    {
        Normal,
        Elite,
        Conditions
    }



    public List<CardConditionWeight> conditionWeights = new List<CardConditionWeight>();

    // Helper class for serializing a CardCondition with a float value
    [System.Serializable]
    public class CardConditionWeight
    {
        public CardMaster.CardCondition condition;
        public float weight;
    }
    [Header("Shop Card Pickups")]
    public List<CardToPickUp> cardPickups = new List<CardToPickUp>(); // Assign in inspector

    

    public override void OnRoomLoaded()
    {
        base.OnRoomLoaded();
        // Get random cards from CardManager and assign to each CardToPickUp
        if (CardManager.instance == null || cardPickups == null || cardPickups.Count == 0)
            return;

        // Assign a random card to each CardToPickUp using CardDatabase.GetRandomCard
        if (shopType == ShopType.Normal)
        {
            // Rarity weights based on player level, now from GameSettings
            float level = ExpBar.Level;
            var gs = GameSettings.instance;
            float cWeight = gs != null ? gs.commonWeight : 1f;
            float uWeight = gs != null ? Mathf.Min(level * gs.uncommonWeightIncrement, gs.uncommonWeightCap) : Mathf.Min(level * 0.04f, 1f);
            float rWeight = gs != null ? Mathf.Min(level * gs.rareWeightIncrement, gs.rareWeightCap) : Mathf.Min(level * 0.03f, 1f);
            float eWeight = gs != null ? Mathf.Min(level * gs.epicWeightIncrement, gs.epicWeightCap) : Mathf.Min(level * 0.015f, 1f);
            float lWeight = gs != null ? Mathf.Min(level * gs.legendaryWeightIncrement, gs.legendaryWeightCap) : Mathf.Min(level * 0.007f, 1f);

            float totalWeight = cWeight + uWeight + rWeight + eWeight + lWeight;

            for (int i = 0; i < cardPickups.Count; i++)
            {
                if (cardPickups[i] != null)
                {
                    float roll = Random.value * totalWeight;
                    CardMaster.CardRarity chosenRarity = CardMaster.CardRarity.Common;
                    if (roll < cWeight)
                        chosenRarity = CardMaster.CardRarity.Common;
                    else if (roll < cWeight + uWeight)
                        chosenRarity = CardMaster.CardRarity.Uncommon;
                    else if (roll < cWeight + uWeight + rWeight)
                        chosenRarity = CardMaster.CardRarity.Rare;
                    else if (roll < cWeight + uWeight + rWeight + eWeight)
                        chosenRarity = CardMaster.CardRarity.Epic;
                    else
                        chosenRarity = CardMaster.CardRarity.Legendary;

                    // Get a card of the chosen rarity
                    var randomCard = CardDatabase.GetRandomCard(card => card.card_rarity == chosenRarity, false);
                    // Fallback to any card if none found
                    if (randomCard == null)
                        randomCard = CardDatabase.GetRandomCard(_ => true, false);
                    if (randomCard != null)
                    {
                        cardPickups[i].SetCard(Instantiate(randomCard));
                    }
                }
            }
        }
        else if (shopType == ShopType.Elite)
        {
            // For Elite shops, you might want to filter for stronger cards
            for (int i = 0; i < cardPickups.Count; i++)
            {
                if (cardPickups[i] != null)
                {
                    var randomCard = CardDatabase.GetRandomCard(card => card.card_rarity != CardMaster.CardRarity.Common, false);
                    if (randomCard != null)
                    {
                        cardPickups[i].SetCard(Instantiate(randomCard));
                    }
                }
            }
        }
        else if (shopType == ShopType.Conditions)
        {
            // For Condition shops, you might want to filter for cards with specific conditions
            for (int i = 0; i < cardPickups.Count; i++)
            {
                if (cardPickups[i] != null)
                {
                    var randomCard = CardDatabase.GetRandomCard(card => card.card_type == CardMaster.CardType.Value || card.card_type == CardMaster.CardType.Action, false);
                    if (randomCard != null)
                    {
                        var cardObject = Instantiate(randomCard);
                        CardMaster cardMaster = cardObject.GetComponent<CardMaster>();
                        foreach (var cond in CardValueConstantRandomGenerator.instance.GetRandomConditions())
                        {
                            if (!cardMaster.card_conditions.Contains(cond))
                            {
                                cardMaster.card_conditions.Add(cond);
                                cardMaster.card_cost = (int)(cardMaster.card_cost * (conditionWeights.Find(cw => cw.condition == cond)?.weight ?? 1f)); // Apply weight if exists
                            }
                        }
                        cardPickups[i].SetCard(cardObject);
                    }
                }
            }
        }
        
    }
}
