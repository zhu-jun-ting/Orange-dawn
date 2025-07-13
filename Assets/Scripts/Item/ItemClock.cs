using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ItemClock: On hit, has a chance to trigger a random action card on board if sufficient mana.
/// Shows a tip about the triggered card or mana insufficient.
/// </summary>
public class ItemClock : ItemMaster
{
    [Header("Clock Settings")]
    [Tooltip("Chance to trigger a random action card (0-1)")]
    public float triggerChance = 0.1f;
    public string tipTriggered = "Triggered: {0}";
    public string tipInsufficient = "not enough mana";

    public override void OnHit(Collision2D collision)
    {
        base.OnHit(collision);
        if (Random.value < triggerChance)
        {
            var actionCards = GetActionCardsOnBoard();
            if (actionCards.Count > 0)
            {
                var card = actionCards[Random.Range(0, actionCards.Count)];
                if (ManaBar.CanCostMana(card.card_cost))
                {
                    card.GetComponent<ICardAction>()?.TriggerAction();
                }
                else
                {
                    ShowTip(GameSettings.AddIcon(tipInsufficient));
                }
            }
        }
    }

    private List<CardMaster> GetActionCardsOnBoard()
    {
        var result = new List<CardMaster>();
        if (BoardArea.instance != null)
        {
            var cards = BoardArea.instance.GetCardsOnBoard();
            foreach (var card in cards)
            {
                if (card.card_type == CardMaster.CardType.Action)
                    result.Add(card);
            }
        }
        return result;
    }
}
