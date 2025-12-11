using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CardValueSharpCleaver : CardMaster
{
    // id: 221
    // name: Sharp Cleaver
    // desc: Each time you kill an enemy with crit, Probability: 20% to add Damage: 1 to this card.

    [Header("Sharp Cleaver Settings")]
    [Tooltip("Chance (0-100) to add damage on critical kill")] 
    public float critKillDamageChance = 20f;
    [Tooltip("Amount of damage to add on trigger")] 
    public int damageAddAmount = 1;

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

        // Check probability to add damage
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll > critKillDamageChance) return;

        // Add damage to this card
        damage += damageAddAmount;
        default_damage += damageAddAmount; // Update initial damage to reflect the gain
        CardMaster.InvokeUpdateCardTexts(); // Update card texts to reflect new damage
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
            string.Format(GameSettings.LocalizeText(card_description), critKillDamageChance, (int)damageAddAmount, (int)damage));
    }
}
