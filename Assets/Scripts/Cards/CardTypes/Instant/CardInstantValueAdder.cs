using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardInstantValueAdder : CardMaster
{
    // 403	Tough - Give STAR card permanent Health: +2
    // 404	Muscle - Give STAR card permanent Damage: +2
    // 405	Lucky - Give STAR card permanent Probability: +5%
    // 406	Duplicate - Give STAR card permanent Amount: +1
    // 407	Wallet - Give STAR card permanent Coin: +3
    // 408	Crystal - Give STAR card permanent Mana: +2
    // 414	Savings - Give STAR card permanent Mana: -1

    public override void OnCardEnable()
    {
        // For each linked card, permanently add all nonzero stat values from this card using UpdateNumberValue
        CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
        foreach (var link in linked)
        {
            if (link != null)
            {
                if (damage != 0) link.UpdateNumberValue(NumberType.Damage, damage, this, true);
                if (health != 0) link.UpdateNumberValue(NumberType.Health, health, this, true);
                if (probability != 0) link.UpdateNumberValue(NumberType.Probability, probability, this, true);
                if (amount != 0) link.UpdateNumberValue(NumberType.Amount, amount, this, true);
                if (mana != 0) link.UpdateNumberValue(NumberType.Mana, mana, this, true);
                if (speed != 0) link.UpdateNumberValue(NumberType.Speed, speed, this, true);
                if (time != 0) link.UpdateNumberValue(NumberType.Time, time, this, true);
                if (coin != 0) link.UpdateNumberValue(NumberType.Coin, coin, this, true);
            }
        }
        // Destroy self after applying
        OnCardDestroyed();
    }

    public override string GetDescription()
    {
        // Build a string like "Permanently give the linked card Damage: XX, Health: YY, ..." for all nonzero stats
        List<string> parts = new List<string>();
        if (damage != 0) parts.Add($"Damage: {damage}");
        if (health != 0) parts.Add($"Health: {health}");
        if (probability != 0) parts.Add($"Probability: {probability}");
        if (amount != 0) parts.Add($"Amount: {amount}");
        if (mana != 0) parts.Add($"Mana: {mana}");
        if (speed != 0) parts.Add($"Speed: {speed}");
        if (time != 0) parts.Add($"Time: {time}");
        if (coin != 0) parts.Add($"Coin: {coin}");
        if (parts.Count == 0) return "No permanent stat increases.";
        string joined = string.Join(", ", parts);
        return GameSettings.AddIcon($"Permanently give linked card(s) {joined}");
    }
}
