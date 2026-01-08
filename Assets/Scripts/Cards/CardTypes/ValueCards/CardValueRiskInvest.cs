using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueRiskInvest : CardMaster
{
    // id: custom
    // name: Risk Invest
    // desc: While this card is active, it has fixed 1% chance to get Amount: +1 and lose that ability.

    [Header("Risk Invest Settings")]
    [Tooltip("Chance (0-100) to trigger the bonus")] 
    public float triggerChance = 1f;
    [Tooltip("Amount gained when triggered")] 
    public int amountGain = 1;

    private bool hasTriggered = false;

    protected override void Awake()
    {
        base.Awake();
        GameEvents.instance.OnLevelCleared += OnLevelClearedHandler;
    }

    private void OnLevelClearedHandler()
    {
        // Only attempt to trigger if we haven't already
        if (hasTriggered) return;

        // Check probability to trigger
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll > triggerChance) return;

        // Trigger the bonus
        amount += amountGain;
        default_amount += amountGain;
        hasTriggered = true;
        CardMaster.InvokeUpdateCardTexts();

        // Unsubscribe from the event since we've lost the ability
        GameEvents.instance.OnLevelCleared -= OnLevelClearedHandler;
    }

    public override bool OnCardDestroyed()
    {
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsEternal)) return false;
        if (GameEvents.instance != null)
            GameEvents.instance.OnLevelCleared -= OnLevelClearedHandler;
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        string statusText = hasTriggered ? " (Triggered)" : "";
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), triggerChance, amountGain, (int)amount)) + statusText;
    }
}
