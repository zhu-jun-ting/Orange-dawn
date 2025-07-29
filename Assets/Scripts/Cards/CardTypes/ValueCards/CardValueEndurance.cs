using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueEndurance : CardMaster
{
    // id: 205
    // name: Endurance
    // When you take damage, 20% chance to gain +5 Health
    // Initial Health: 5

    [Header("Endurance Settings")]
    [Tooltip("Chance (0-1) to gain health when taking damage")] 
    public float gainHealthChance = 0.2f;
    [Tooltip("Amount of health gained on trigger")] 
    public int healthGainAmount = 5;

    private System.Random rng = new System.Random();

    protected override void Awake()
    {
        base.Awake();
        // Subscribe to OnHitPawn event
        GameEvents.instance.OnHitPawn += OnHitPawnHandler;
    }

    private void OnHitPawnHandler(float damage, PawnMaster receiver, GameObject instigator, GameEvents.DamageType damageType, Transform location, float hitBackFactor, Gun source)
    {
        // Check if receiver is the player and this card is in play (on board or in hand)
        if (receiver != null && receiver.isPlayer)
        {
            // 20% chance to gain health
            if (rng.NextDouble() < gainHealthChance)
            {
                health += healthGainAmount;
                default_health += healthGainAmount; // Update initial health to reflect the gain
                CardMaster.InvokeUpdateCardTexts(); // Update card texts to reflect new health
            }
        }
    }

    public override bool OnCardDestroyed()
    {
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsEternal)) return false;
        // Unsubscribe to prevent memory leaks
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= OnHitPawnHandler;
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(
            string.Format(GameSettings.LocalizeText(card_description), gainHealthChance * 100, (int)healthGainAmount, (int)health));
    }
}
