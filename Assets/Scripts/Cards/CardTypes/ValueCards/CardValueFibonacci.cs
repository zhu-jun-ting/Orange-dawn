using System.Collections.Generic;
using UnityEngine;

public class CardValueFibonacci : CardMaster
{
    [Header("Fibonacci Damage Settings")]
    public float increment = 0.1f;
    private int lastFibonacci = 1;
    private float damageMultiplier = 1f;

    private static readonly List<int> fibonacciList = new List<int> { 2, 3, 5, 8, 13, 21, 34, 55, 89, 144, 233, 377, 610, 987, 1597, 2584, 4181, 6765 };
    private int fibIndex = 0;

    private void OnEnable()
    {
        GameEvents.instance.OnHitPawn += OnHitPawn;
        ApplyMultiplierToLinkedCards();
    }

    private void OnDisable()
    {
        GameEvents.instance.OnHitPawn -= OnHitPawn;
    }

    private void OnHitPawn(float damage_, PawnMaster receiver_, GameObject instigator_, GameEvents.DamageType damageType_, Transform location_, float hitBackFactor_, Gun source_)
    {
        if (receiver_ != null && !receiver_.isPlayer)
        {
            // Check if damage matches next Fibonacci number
            if (fibIndex < fibonacciList.Count && (int)damage_ == fibonacciList[fibIndex])
            {
                fibIndex++;
                damageMultiplier += increment;
                PlayerController.ShowPopup($"Fibonacci reached: {fibonacciList[fibIndex - 1]}");
                CardMaster.InvokeUpdateCardTexts();
            }
        }
    }

    private void ApplyMultiplierToLinkedCards()
    {
        // Apply damageMultiplier to all linked cards
        CardMaster[] linked = new CardMaster[] { up_link_cardmaster, left_link_cardmaster, right_link_cardmaster, down_link_cardmaster };
        foreach (var link in linked)
        {
            if (link != null)
            {
                link.UpdateNumberValue(NumberType.Damage, damageMultiplier, this, true, true); // Permanent multiply
            }
        }
    }

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        ApplyMultiplierToLinkedCards();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), increment, damageMultiplier, fibonacciList[fibIndex]));
    }
}
