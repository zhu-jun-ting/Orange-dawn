using System;
using UnityEngine;

public class CardInstantLogOnTrigger : CardMaster
{
    [Header("Debug Logger Settings")]
    public int manaCost = 1;
    public float triggerProbability = 1f;
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

                // update parent mana cost
                link.UpdateSelfNumberValue(CardMaster.NumberType.Mana, manaCost, isPermanent: true);
            }
        }
        // Only destroy if at least one action was found (otherwise, card can be re-enabled later)
        if (foundAction)
            OnCardDestroyed();
    }

    private void LogOnTrigger(CardMaster card, Transform location)
    {
        if (UnityEngine.Random.value < triggerProbability) return; // Check trigger probability
        Debug.Log($"[Debug Logger] Triggered by card: {card?.card_name ?? "Unknown"} at position: {location?.position ?? Vector3.zero}. Mana cost: {manaCost}");
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, manaCost));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(string.Format(buffName, $"{triggerProbability * 100:0}%"));
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(buffDescription);
    }
}
