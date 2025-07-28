using System;
using UnityEngine;


/// <summary>
/// This is the parent class for Sticker Cards that can attach to another action card to do something. 
/// Override the ActionOnTrigger() to define its own logic when triggered. 
/// </summary>
public class CardInstantStickerMaster : CardMaster
{
    // [Header("Buff Entry Text")]
    // public string buffName = "Sticker Name";
    // [TextArea(3, 10)]
    // private string buffDescription = "Sticker Description";

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
                action.OnTrigger -= ActionOnTrigger;
                action.OnTrigger += ActionOnTrigger;

                // Also give a new buff entry
                buffEntry = link.AddBuffEntry(GetBuffEntryName(), GetBuffEntryText(), 0);

                // Register to update buffEntry's name and description when card texts update
                CardMaster.OnUpdateCardTexts += UpdateBuffEntry;
            }
        }
        // Only destroy if at least one action was found (otherwise, card can be re-enabled later)
        if (foundAction)
        {
            SoundManager.PlaySFX("GetCard");
            OnCardDestroyed();
        }
    
        else
        {
            // If no action was found, show prompt
            ShowPopup("Not An Action.");
        }   
    }

    /// <summary>
    /// Override this to implement the actual logic when the card is triggered.
    /// </summary>
    /// <param name="card">The card that triggered action</param>
    /// <param name="location">The location where the action was triggered</param>
    public virtual void ActionOnTrigger(CardMaster card, Transform location)
    {
        if (UnityEngine.Random.value < probability / 100) return; // Check trigger probability
        Debug.Log($"[Debug Logger] Triggered by card: {card?.card_name ?? "Unknown"} at position: {location?.position ?? Vector3.zero}. Mana cost: {mana}");
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, mana));
    }

    /// <summary>
    /// Override this to provide a custom name for the buff entry.
    /// </summary>
    /// <returns></returns>
    public override string GetBuffEntryName()
    {
        return "";
    }

    /// <summary>
    /// Override this to provide a custom text for the buff entry.
    /// </summary>
    /// <returns></returns>
    public override string GetBuffEntryText()
    {
        return "";
    }

    public void OnDestroy()
    {
        // Unsubscribe from the OnUpdateCardTexts event to prevent memory leaks
        CardMaster.OnUpdateCardTexts -= UpdateBuffEntry;
    }
}
