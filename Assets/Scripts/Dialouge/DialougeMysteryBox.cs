using UnityEngine;

/// <summary>
/// DialougeMysteryBox: Handles mystery box event actions for NPC dialogue events.
/// </summary>
public class DialougeMysteryBox : EventNPCDialogue
{
    // Mystery box logic: change a card based on coin parity
    public void MysteryBoxChangeCard()
    {
        var player = PlayerController.instance;
        if (player == null || HandArea.instance == null) return;

        int currentCoin = CoinCounter.coinCurrent;
        var handCards = HandArea.instance.GetCardsOnHand();
        handCards.RemoveAll(card => card.card_conditions.Contains(CardMaster.CardCondition.IsEternal));
        if (handCards.Count == 0) return;

        var cardToReplace = handCards[Random.Range(0, handCards.Count)];
        if (cardToReplace == null) return;

        var rarity = cardToReplace.card_rarity;
        GameObject newCardPrefab = null;
        if (currentCoin % 2 == 1) // Odd: same or less rarity
        {
            newCardPrefab = CardDatabase.GetRandomCard(card => card.card_rarity <= rarity, false);
        }
        else // Even: same or more rarity
        {
            newCardPrefab = CardDatabase.GetRandomCard(card => card.card_rarity >= rarity, false);
        }

        // Destroy the old card
        cardToReplace.OnCardDestroyed();

        if (newCardPrefab != null)
        {
            CardManager.instance.QueueAddCardObjects(new System.Collections.Generic.List<GameObject> { GameObject.Instantiate(newCardPrefab) });
            PlayerController.ShowPopup($"Mystery Box: {cardToReplace.card_name} → {newCardPrefab.GetComponent<CardMaster>().card_name}");
        }
    }

    // Example dialogue setup (to be assigned in the inspector or via code)
    [Header("Sample Mystery Box Dialogue")]
    [TextArea]
    public string mysteryBoxDialogue = "A mysterious box sits before you. Will you risk changing your fate?";
    [TextArea]
    public string[] mysteryBoxChoices = new string[]
    {
        "Open the Mystery Box",
        "Leave"
    };
}
