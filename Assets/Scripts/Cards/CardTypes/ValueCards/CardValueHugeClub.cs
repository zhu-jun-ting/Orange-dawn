using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueHugeClub : CardMaster
{
    // id: custom
    // name: Huge Club
    // desc: Give Amount: +1 if this card has Damage: >=30
    // Damage: 1

    [Header("Huge Club Settings")]
    [Tooltip("Damage threshold to trigger the bonus")] 
    public float damageTreshold = 30f;
    [Tooltip("Amount bonus when damage threshold is met")] 
    public int amountBonus = 1;

    private bool isBonusActive = false;

    protected override void Awake()
    {
        base.Awake();
        CardMaster.OnUpdateCardValues += OnCardValuesUpdate;
        ApplyAmountBonus();
    }

    private void OnCardValuesUpdate()
    {
        ApplyAmountBonus();
    }

    private void ApplyAmountBonus()
    {
        bool shouldActivateBonus = damage >= damageTreshold;

        if (shouldActivateBonus && !isBonusActive)
        {
            // Activate bonus
            amount += amountBonus;
            default_amount += amountBonus;
            isBonusActive = true;
            CardMaster.InvokeUpdateCardTexts();
        }
        else if (!shouldActivateBonus && isBonusActive)
        {
            // Deactivate bonus
            amount -= amountBonus;
            default_amount -= amountBonus;
            isBonusActive = false;
            CardMaster.InvokeUpdateCardTexts();
        }
    }

    public override bool OnCardDestroyed()
    {
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsEternal)) return false;
        CardMaster.OnUpdateCardValues -= OnCardValuesUpdate;
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), (int)damageTreshold, amountBonus, (int)amount, (int)damage));
    }
}
