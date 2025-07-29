using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantPermanentIncrease : CardMaster
{
    // Permanent Increase Settings
    [Tooltip("All nonzero stat values on this card will be permanently added to linked cards.")]
    // No extra fields needed; use base stat fields (damage, (int)health, etc)

    public override void OnCardEnable()
    {
        // For each linked card, permanently add all nonzero stat values from this card using UpdateNumberValue
        CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
        foreach (var link in linked)
        {
            if (link != null)
            {
                if (damage != 0) link.UpdateNumberValue(NumberType.Damage, (int)damage, this, true);
                if (health != 0) link.UpdateNumberValue(NumberType.Health, (int)health, this, true);
                if (probability != 0) link.UpdateNumberValue(NumberType.Probability, probability, this, true);
                if (amount != 0) link.UpdateNumberValue(NumberType.Amount, (int)amount, this, true);
                if (mana != 0) link.UpdateNumberValue(NumberType.Mana, (int)mana, this, true);
                if (coin != 0) link.UpdateNumberValue(NumberType.Coin, coin, this, true);
            }
        }
        // Destroy self after applying
        SoundManager.PlaySFX("GetCard");
        OnCardDestroyed();
    }

    public override string GetDescription()
    {
        // Build a string like "Permanently give the linked card Damage: XX, (int)health: YY, ..." for all nonzero stats
        List<string> parts = new List<string>();
        if (damage != 0) parts.Add($"Damage: {damage}");
        if (health != 0) parts.Add($"Health: {health}");
        if (probability != 0) parts.Add($"Probability: {probability}");
        if (amount != 0) parts.Add($"Amount: {amount}");
        if (mana != 0) parts.Add($"Mana: {mana}");
        if (coin != 0) parts.Add($"Coin: {coin}");
        if (parts.Count == 0) return "No permanent stat increases.";
        string joined = string.Join(", ", parts);
        return GameSettings.AddIcon($"Permanently give the linked card {joined}");
    }
}
