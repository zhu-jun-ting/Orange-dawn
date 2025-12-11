using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueBloodCleaver : CardMaster
{
    // id: 222
    // name: Blood Cleaver
    // desc: Each time you kill an enemy with crit, Probability: 20% to add Health: 2 to this card.

    [Header("Blood Cleaver Settings")]
    [Tooltip("Amount of health to add on trigger")] 
    public int healthAddAmount = 2;

    protected override void Awake()
    {
        base.Awake();
        // Subscribe to OnPawnDie event
        GameEvents.instance.OnPawnDie += OnPawnDieHandler;
    }

    private void OnPawnDieHandler(PawnMaster deadPawn, float damageDealt, GameObject instigator, GameEvents.DamageType damageType, Gun source)
    {
        // Check if the dead pawn is an enemy
        EnemyMaster enemy = deadPawn as EnemyMaster;
        if (enemy == null) return;

        // Check if death was from a critical hit
        if (damageType != GameEvents.DamageType.Crit) return;

        // Check probability to add health
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll > probability) return;

        // Add health to this card
        health += healthAddAmount;
        default_health += healthAddAmount; // Update initial health to reflect the gain
        CardMaster.InvokeUpdateCardTexts(); // Update card texts to reflect new health
    }

    public override bool OnCardDestroyed()
    {
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsEternal)) return false;
        // Unsubscribe to prevent memory leaks
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie -= OnPawnDieHandler;
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(
            string.Format(GameSettings.LocalizeText(card_description), probability, (int)healthAddAmount, (int)health));
    }
}
