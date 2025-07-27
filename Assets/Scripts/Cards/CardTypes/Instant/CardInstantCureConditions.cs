using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// CardInstantCureConditions: Removes XX negative conditions from linked cards. If XX == -1, removes all negative conditions.
/// </summary>
public class CardInstantCureConditions : CardMaster
{
    [Header("Cure Settings")]
    [Tooltip("Number of negative conditions to remove (-1 = all)")]
    public int cureCount = 1;

    // List of negative conditions (from parent CardMaster)
    private static readonly List<CardCondition> negativeConditions = new List<CardCondition>
    {
        CardCondition.IsFrail,
        CardCondition.IsFragile,
        CardCondition.IsTemporary,
        CardCondition.IsVolatile,
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
            foreach (var cond in negativeConditions)
            {
                if (link.card_conditions.Contains(cond))
                {
                    toRemove.Add(cond);
                    removed++;
                    if (cureCount != -1 && removed >= cureCount)
                        break;
                }
            }
            foreach (var cond in toRemove)
            {
                link.RemoveCondition(cond);
            }
        }
        // Destroy self after applying
        SoundManager.PlaySFX("GetCard");
        OnCardDestroyed();
    }

    public override string GetDescription()
    {
        // Format condition names for display
        var condNames = new List<string>();
        foreach (var cond in negativeConditions)
        {
            condNames.Add(GameSettings.GetConditionName(cond));
        }
        string condList = string.Join(", ", condNames);
        if (cureCount == -1)
            return GameSettings.AddIcon(string.Format(card_description));
            // return GameSettings.AddIcon($"Remove all negative conditions ({condList}) from linked card(s)");
        else
            return GameSettings.AddIcon(string.Format(card_description, cureCount));
            // return GameSettings.AddIcon($"Remove up to {cureCount} negative conditions from linked card(s)");
    }
    
}
