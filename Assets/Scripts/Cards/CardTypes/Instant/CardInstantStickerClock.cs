using System;
using UnityEngine;
using System.Collections.Generic;

/// <summary>
/// Sticker card that attaches to an Action Card and, with 30% probability, triggers another card on the board when triggered.
/// </summary>
public class CardInstantStickerClock : CardInstantStickerMaster
{
    [Header("Sticker Clock Settings")]
    [Range(0f, 1f)] public float triggerProbability = 0.1f;

    [Header("Buff Entry Text")]
    public string clockBuffName = "Clock Sticker";
    [TextArea(3, 10)]
    public string clockBuffDescription = "When this Action Card triggers, Probability: {0} to trigger another card on board.";

    public override void ActionOnTrigger(CardMaster card, Transform target)
    {
        if (UnityEngine.Random.value > triggerProbability) return;
        var cards = BoardArea.instance != null ? BoardArea.instance.GetCardsOnBoard() : null;
        if (cards == null || cards.Count == 0) return;
        // Filter to only ICardAction cards that are not the triggering card
        List<CardMaster> candidates = new List<CardMaster>();
        foreach (var c in cards)
        {
            if (c != card && c is ICardAction)
            {
                candidates.Add(c);
            }
        }
        if (candidates.Count == 0) return;
        CardMaster randomCard = candidates[UnityEngine.Random.Range(0, candidates.Count)];
        // Trigger its TriggerAction()
        (randomCard as ICardAction).TriggerAction(randomCard, target);
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(clockBuffDescription, (int) (triggerProbability*100)));
    }

    public override string GetBuffEntryName()
    {
        return GameSettings.AddIcon(clockBuffName);
    }

    public override string GetBuffEntryText()
    {
        return GameSettings.AddIcon(string.Format(clockBuffDescription, (int) (triggerProbability*100)));
    }
}
