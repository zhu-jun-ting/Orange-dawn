using System;
using UnityEngine;

public class CardInstantLogOnTrigger : CardMaster
{
    [Header("Buff Entry Text")]
    public string buffName = "Debug Logger ({0})";
    [TextArea(3, 10)]
    public string buffDescription = "Log when attach card is triggered";

    public override void OnCardEnable()
    {
        CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
        bool foundAction = false;
        foreach (var link in linked)
        {
            if (link != null && link is ICardAction action)
            {
                foundAction = true;

                // Remove previous to avoid stacking
                action.OnTrigger -= LogOnTrigger;
                action.OnTrigger += LogOnTrigger;

                // Also give a new buff entry
                buffEntry = link.AddBuffEntry(GetBuffEntryName(), GetBuffEntryText(), 0);

                // Register to update buffEntry's name and description when card texts update
                CardMaster.OnUpdateCardTexts += UpdateBuffEntry;

                // update parent values
                if (damage != 0) link.UpdateNumberValue(NumberType.Damage, damage, this, true);
                if (health != 0) link.UpdateNumberValue(NumberType.Health, health, this, true);
                // if (probability != 0) link.UpdateNumberValue(NumberType.Probability, probability, this, true);
                if (amount != 0) link.UpdateNumberValue(NumberType.Amount, amount, this, true);
                if (mana != 0) link.UpdateNumberValue(NumberType.Mana, mana, this, true);
                if (coin != 0) link.UpdateNumberValue(NumberType.Coin, coin, this, true);
            }
        }
        // Only destroy if at least one action was found (otherwise, card can be re-enabled later)
        if (foundAction)
        {
            SoundManager.PlaySFX("GetCard");
            OnCardDestroyed();            
        }

    }

    private void LogOnTrigger(CardMaster card, Transform location)
    {
        if (UnityEngine.Random.value < probability) return; // Check trigger probability
        Debug.Log($"[Debug Logger] Triggered by card: {card?.card_name ?? "Unknown"} at position: {location?.position ?? Vector3.zero}. Mana cost: {mana}");
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, mana));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(string.Format(buffName));
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(buffDescription, probability));
    }
}
