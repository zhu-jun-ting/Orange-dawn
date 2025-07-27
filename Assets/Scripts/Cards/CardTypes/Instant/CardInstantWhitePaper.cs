using System.Collections.Generic;
using UnityEngine;

// id: 410
// name: White Paper
// desc: Remove all tags on the card. Each condition removed, give you 3 COIN
    
public class CardInstantWhitePaper : CardMaster
{
    [Header("White Paper Settings")]
    [Tooltip("Coin gained per condition removed")]
    public int coinPerCondition = 1;

    // List of all conditions (from parent CardMaster)
    private static readonly List<CardCondition> allConditions = new List<CardCondition>
    {
        CardCondition.IsUndraggable,
        CardCondition.IsPowerful,
        CardCondition.IsFrail,
        CardCondition.IsFragile,
        CardCondition.IsTemporary,
        CardCondition.IsVolatile,
        CardCondition.IsGrowing,
        CardCondition.IsDecaying
    };

    public override void OnCardEnable()
    {
        CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
        foreach (var link in linked)
        {
            if (link == null) continue;
            int removed = 0;
            var toRemove = new List<CardCondition>();
            foreach (var cond in allConditions)
            {
                if (link.card_conditions.Contains(cond))
                {
                    toRemove.Add(cond);
                }
            }
            foreach (var cond in toRemove)
            {
                link.RemoveCondition(cond);
                removed++;
            }
            // Gain coin for each condition removed
            if (removed > 0 && GameEvents.instance != null)
            {
                GameEvents.instance.UpdateCoins(removed * coinPerCondition);
                ShowPopup(GameSettings.AddIcon($"{removed * coinPerCondition} COIN."));
            }
        }
        // Destroy self after applying
        SoundManager.PlaySFX("GetCard");
        OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description));
    }
}
