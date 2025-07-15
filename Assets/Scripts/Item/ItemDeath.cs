using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// ItemDeath: When destroyed, destroys a random card in hand.
/// </summary>
public class ItemDeath : ItemMaster
{
    [Header("Death Settings")]
    public string tip = "Destroyed {0}";

    public override void OnItemDestroyed(Collision2D collision)
    {
        base.OnItemDestroyed(collision);
        DestroyRandomHandCard();
    }

    private void DestroyRandomHandCard()
    {
        if (HandArea.instance != null)
        {
            var handCards = HandArea.instance.GetCardsOnHand();
            handCards.RemoveAll(card => card.card_conditions.Contains(CardMaster.CardCondition.IsEternal));
            if (handCards.Count > 0)
            {
                var card = handCards[Random.Range(0, handCards.Count)];
                if (card != null)
                {
                    card.OnCardDestroyed();
                    ShowTip(GameSettings.AddIcon(string.Format(tip, card.card_name)));
                }
            }
        }
    }
}
