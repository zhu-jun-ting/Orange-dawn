using UnityEngine;
using System.Collections.Generic;

public class ItemWonderBox : ItemMaster
{
    [Header("WonderBox Settings")]
    [Tooltip("Chance to inflict a random condition on a card (0-1)")]
    public float conditionChance = 0.1f;
    [Tooltip("Possible conditions to inflict")]
    public List<CardMaster.CardCondition> possibleConditions = new List<CardMaster.CardCondition> {
        CardMaster.CardCondition.IsPowerful,
        CardMaster.CardCondition.IsFrail,
        CardMaster.CardCondition.IsFragile,
        CardMaster.CardCondition.IsTemporary,
        CardMaster.CardCondition.IsVolatile
    };

    public override void OnHit(Collision2D collision)
    {
        base.OnHit(collision);
        if (Random.value < conditionChance)
        {
            CardMaster targetCard = FindRandomCardOnBoard();
            if (targetCard != null && possibleConditions.Count > 0)
            {
                var condition = possibleConditions[Random.Range(0, possibleConditions.Count)];
                bool added = targetCard.AddCondition(condition);
                if (added)
                {
                    ShowTip($"{targetCard.card_name} Inflicted {condition.ToString()}!");
                }
                else
                {
                    ShowTip($"{targetCard.card_name} {condition.ToString()} already present.");
                }
            }
        }
    }

    private CardMaster FindRandomCardOnBoard()
    {
        // Find all CardMaster instances on board
        if (BoardArea.instance != null)
        {
            var boardCards = BoardArea.instance.GetCardsOnBoard();
            if (boardCards.Count == 0) return null;
            return boardCards[Random.Range(0, boardCards.Count)];
        }
        return null;
    }
}
