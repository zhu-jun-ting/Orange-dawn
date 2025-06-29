using System;
using UnityEngine;

public class CardInstantLogOnTrigger : CardMaster
{
    [Header("Debug Logger Settings")]
    public int manaCost = 1;

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
                link.AddBuffEntry($"Debug Logger ({manaCost})", $"Log when attach card is triggered");
            }
        }
        // Only destroy if at least one action was found (otherwise, card can be re-enabled later)
        if (foundAction)
            OnCardDestroyed();
    }

    private void LogOnTrigger(CardMaster card, Transform location)
    {
        if (!ManaBar.CanCostMana(-manaCost)) return;
        Debug.Log($"[Debug Logger] Triggered by card: {card?.card_name ?? "Unknown"} at position: {location?.position ?? Vector3.zero}. Mana cost: {manaCost}");
        GameEvents.instance?.UpdateMana(-manaCost);
    }

    public override string GetDescription()
    {
        return string.Format(card_description, manaCost);
    }
}
