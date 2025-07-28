using UnityEngine;
using System.Collections.Generic;

public class ItemLootBox : ItemMaster
{
    [Header("LootBox Settings")]
    [Tooltip("How many cards to give when destroyed")] 
    public int cardCount = 1;
    public string tip = "New Card";

    [Header("Rarity Chances (sum does not need to be 1)")]
    [Tooltip("Chance to get a Common card")] public float commonChance = 70f;
    [Tooltip("Chance to get a Uncommon card")] public float uncommonChance = 15f;
    [Tooltip("Chance to get a Rare card")] public float rareChance = 5f;
    [Tooltip("Chance to get an Epic card")] public float epicChance = 8f;
    [Tooltip("Chance to get a Legendary card")] public float legendaryChance = 2f;


    // Called when the item is destroyed (e.g., by animation event or maxHits reached)
    public override void OnItemDestroyed(Collision2D collision)
    {
        GiveLoot();
    }

    private void GiveLoot()
    {
        if (CardDatabase.instance == null) return;
        var cardsToGiveDb = new List<GameObject>();
        for (int i = 0; i < cardCount; i++)
        {
            CardMaster.CardRarity rarity = RollRarity();
            var found = CardDatabase.FindCards(card => card.card_rarity == rarity);
            if (found != null && found.Count > 0)
            {
                int idx = Random.Range(0, found.Count);
                cardsToGiveDb.Add(found[idx]);
            }
            else // fallback: any card
            {
                var all = CardDatabase.FindCards(card => true);
                if (all != null && all.Count > 0)
                {
                    int idx = Random.Range(0, all.Count);
                    cardsToGiveDb.Add(all[idx]);
                }
            }
        }
        if (cardsToGiveDb.Count > 0 && CardManager.instance != null)
        {
            CardManager.instance.QueueAddCardObjects(cardsToGiveDb);
            ShowMessageLocal(GameSettings.AddIcon(tip));
        }
    }

    private CardMaster.CardRarity RollRarity()
    {
        float total = commonChance + uncommonChance + rareChance + epicChance + legendaryChance;
        float roll = Random.Range(0f, total);
        if (roll < commonChance) return CardMaster.CardRarity.Common;
        roll -= commonChance;
        if (roll < uncommonChance) return CardMaster.CardRarity.Uncommon;
        roll -= uncommonChance;
        if (roll < rareChance) return CardMaster.CardRarity.Rare;
        roll -= rareChance;
        if (roll < epicChance) return CardMaster.CardRarity.Epic;
        return CardMaster.CardRarity.Legendary;
    }
}
