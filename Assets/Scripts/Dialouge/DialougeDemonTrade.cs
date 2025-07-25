using UnityEngine;
using DialogueEditor;

/// <summary>
/// DialougeDemonTrade: Handles demon trade event actions for NPC dialogue events.
/// </summary>
public class DialougeDemonTrade : EventNPCDialogue
{
    // Sacrifice 10 Max Health Points and give a rare card
    public void SacrificeMaxHealthForRareCard()
    {
        var player = PlayerController.instance;
        if (player != null)
        {
            // Reduce max health
            HealthBar.HealthGlobalModifier -= 10f;
            if (HealthBar.HealthCurrent + HealthBar.HealthGlobalModifier > HealthBar.HealthMax + HealthBar.HealthGlobalModifier)
                HealthBar.HealthCurrent = HealthBar.HealthMax + HealthBar.HealthGlobalModifier;
            // Give a rare card
            var cardPrefab = CardDatabase.GetRandomCard(card => card.card_rarity == CardMaster.CardRarity.Rare, false);
            if (cardPrefab != null)
            {
                CardManager.instance.QueueAddCardObjects(new System.Collections.Generic.List<GameObject> { GameObject.Instantiate(cardPrefab) });
            }
            PlayerController.ShowPopup("-10 Max Health");
        }
    }

    // Sacrifice a card to gain 10 Max Health
    public void SacrificeCardForMaxHealth()
    {
        var player = PlayerController.instance;
        if (player != null && HandArea.instance != null)
        {
            var handCards = HandArea.instance.GetCardsOnHand();
            handCards.RemoveAll(card => card.card_conditions.Contains(CardMaster.CardCondition.IsEternal));
            if (handCards.Count > 0)
            {
                var card = handCards[Random.Range(0, handCards.Count)];
                if (card != null)
                {
                    card.OnCardDestroyed();
                    HealthBar.HealthGlobalModifier += 10f;
                    PlayerController.ShowPopup($"Sacrificed {card.card_name}");
                    PlayerController.ShowPopup("+10 Max Health");
                }
            }
        }
    }

    // Example dialogue setup (to be assigned in the inspector or via code)
    [Header("Sample Demon Trade Dialogue")]
    [TextArea]
    public string demonTradeDialogue = "A demon offers you a deal. Will you sacrifice your vitality for power, or your power for vitality?";
    [TextArea]
    public string[] demonTradeChoices = new string[]
    {
        "Sacrifice 10 Max Health for a rare card",
        "Sacrifice a card to gain 10 Max Health",
        "Leave"
    };
}
