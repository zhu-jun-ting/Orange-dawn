using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: 507
// name: Immune
// desc: When you take fatal damage, recover you to full health and kill all enemies around you. Then destory this card.

public class CardRelicImmune : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        if (GameEvents.instance != null)
            GameEvents.instance.OnCheckDeathPrevention += HandleCheckDeathPrevention;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
    }

    public override void OnCardDisable()
    {
        base.OnCardDisable();
        if (GameEvents.instance != null)
            GameEvents.instance.OnCheckDeathPrevention -= HandleCheckDeathPrevention;
        OnTrigger -= TriggerAction;
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnCheckDeathPrevention -= HandleCheckDeathPrevention;
        OnTrigger -= TriggerAction;
    }

    private void HandleCheckDeathPrevention(PawnMaster pawn, ref bool prevented)
    {
        // Only prevent death for the player (or owner of this card if we tracked owner properly, but usually relics are player only)
        if (pawn.CompareTag("Player") && !prevented)
        {
            prevented = true;
            OnTrigger?.Invoke(this, pawn.transform);
        }
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        
        // 1. Recover to full health
        if (PlayerController.instance != null)
        {
            // Calculate missing health to heal to full
            float maxHealth = HealthBar.HealthMax;
            float currentHealth = HealthBar.HealthCurrent;
            float healAmount = maxHealth - currentHealth;
            
            if (healAmount > 0)
            {
                GameEvents.instance.HealPawn(healAmount, PlayerController.instance, gameObject, target);
            }
        }

        // 2. Kill all enemies around
        foreach (var enemy in GameObject.FindGameObjectsWithTag("Enemy"))
        {
            if (enemy != null)
            {
                PawnMaster enemyPawn = enemy.GetComponent<PawnMaster>();
                if (enemyPawn != null)
                {
                    // Deal massive damage to trigger death logic (drops, events, etc.)
                    enemyPawn.TakeDamage(999999f, enemyPawn, gameObject, GameEvents.DamageType.Normal, enemy.transform, 0f, null);
                }
                else
                {
                    Destroy(enemy);
                }
            }
        }

        // 3. Destroy this card
        OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(GameSettings.LocalizeText(card_description));
    }
}
