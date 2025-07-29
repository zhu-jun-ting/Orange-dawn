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
        bool modified = false;
        foreach (var link in linked)
        {
            if (link != null)
            {
                if (damage != 0 && link.numberTypesCanBeModified.Contains(NumberType.Damage)) link.UpdateNumberValue(NumberType.Damage, (int)damage, this, true); modified = true;
                if (health != 0 && link.numberTypesCanBeModified.Contains(NumberType.Health)) link.UpdateNumberValue(NumberType.Health, (int)health, this, true); modified = true;
                if (probability != 0 && link.numberTypesCanBeModified.Contains(NumberType.Probability)) link.UpdateNumberValue(NumberType.Probability, probability, this, true); modified = true;
                if (amount != 0 && link.numberTypesCanBeModified.Contains(NumberType.Amount)) link.UpdateNumberValue(NumberType.Amount, (int)amount, this, true); modified = true;
                if (mana != 0 && link.numberTypesCanBeModified.Contains(NumberType.Mana)) link.UpdateNumberValue(NumberType.Mana, (int)mana, this, true); modified = true;
                if (coin != 0 && link.numberTypesCanBeModified.Contains(NumberType.Coin)) link.UpdateNumberValue(NumberType.Coin, coin, this, true); modified = true;
            }
        }
        // Destroy self after applying
        if (modified)
        {
            SoundManager.PlaySFX("GetCard");
            OnCardDestroyed();
        }
        else ShowPopup("No stat to increase.");
    }

    public override string GetDescription()
    {
        // Build a string like "Permanently give the linked card Damage: XX, (int)health: YY, ..." for all nonzero stats
        List<string> parts = new List<string>();
        if (damage != 0) parts.Add($"Damage: {(int)damage}");
        if (health != 0) parts.Add($"Health: {(int)health}");
        if (probability != 0) parts.Add($"Probability: {(int)probability}");
        if (amount != 0) parts.Add($"Amount: {(int)amount}");
        if (mana != 0) parts.Add($"Mana: {(int)mana}");
        if (coin != 0) parts.Add($"Coin: {(int)coin}");
        if (parts.Count == 0) return "No permanent stat increases.";
        string joined = string.Join(", ", parts);
        return GameSettings.AddIcon($"{GameSettings.LocalizeText("ValueAdder_desc")} {joined}");
    }
}
