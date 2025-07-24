using UnityEngine;

/// <summary>
/// DialougeRunestone: Handles runestone-specific event actions for NPC dialogue events.
/// </summary>
public class DialougeRunestone : EventNPCDialogue
{
    // Call this to add a random value card to the player's hand
    public void AddRandomValueCard()
    {
        var cardPrefab = CardDatabase.GetRandomCard(card => card.card_type == CardMaster.CardType.Value, false);
        if (cardPrefab != null)
        {
            CardManager.instance.QueueAddCardObjects(new System.Collections.Generic.List<GameObject> { GameObject.Instantiate(cardPrefab) });
        }
    }

    // Call this to destroy a random card and give player 30 coins
    public void DestroyRandomCardAndGiveCoins()
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
                    GameEvents.instance.UpdateCoins(30);
                    PlayerController.ShowPopup(string.Format("Destroyed {0}", card.card_name));
                }
            }
        }
    }
}
