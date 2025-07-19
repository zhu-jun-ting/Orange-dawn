using System;
using UnityEngine;

/// <summary>
/// Sticker card that attaches to an Action Card and gives Mana when triggered.
/// </summary>
public class CardInstantStickerMana : CardInstantStickerMaster
{
    [Header("Sticker Mana Settings")]
    public int giveMana = 1;

    [Header("Buff Entry Text")]
    public string manaBuffName = "Mana Sticker";
    [TextArea(3, 10)]
    public string manaBuffDescription = "When this Action Card triggers, gain Mana: {0}";

    /// <summary>
    /// Override to implement mana gain when the action card is triggered.
    /// </summary>
    public override void ActionOnTrigger(CardMaster card, Transform location)
    {
        // if (UnityEngine.Random.value < probability / 100) return; // Check trigger probability if used
        if (PlayerController.instance != null)
        {
            GameEvents.instance.UpdateMana(giveMana);
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(manaBuffDescription, giveMana));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(manaBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(manaBuffDescription, giveMana));
    }
}
