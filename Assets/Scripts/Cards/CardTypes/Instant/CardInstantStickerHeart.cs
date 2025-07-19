using System;
using UnityEngine;

/// <summary>
/// Sticker card that attaches to an Action Card and recovers Health when triggered.
/// </summary>
public class CardInstantStickerHeart : CardInstantStickerMaster
{
    [Header("Sticker Heart Settings")]
    public int recoverHealth = 3;

    [Header("Buff Entry Text")]
    public string heartBuffName = "Heart Sticker";
    [TextArea(3, 10)]
    public string heartBuffDescription = "When this Action Card triggers, recover your Health: {0}";

    /// <summary>
    /// Override to implement health recovery when the action card is triggered.
    /// </summary>
    public override void ActionOnTrigger(CardMaster card, Transform location)
    {
        // if (UnityEngine.Random.value < probability / 100) return; // Check trigger probability if used
        if (PlayerController.instance != null)
        {
            GameEvents.instance.HealPawn(recoverHealth, PlayerController.instance, null, PlayerController.instance.transform);
        }
        // Debug.Log($"[StickerMana] Triggered by card: {card?.card_name ?? "Unknown"}. Recovered Health: {recoverHealth}");
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, recoverHealth));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(heartBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(heartBuffDescription, recoverHealth));
    }
}
