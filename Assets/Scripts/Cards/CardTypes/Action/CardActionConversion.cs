using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: custom
// name: Conversion
// desc: When you kill an enemy with a crit hit, Probability: 60% to heal you for the damage dealt. Cost: Mana: 3
public class CardActionConversion : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event System.Action<CardMaster, Transform> OnTrigger;

    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie += HandleOnPawnDie;
        OnTrigger -= TriggerAction; // Unsubscribe to avoid duplicates
        OnTrigger += TriggerAction; // Subscribe to the trigger event
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    private void HandleOnPawnDie(PawnMaster deadPawn, float killDamage, GameObject instigator, GameEvents.DamageType damageType, Gun source)
    {
        // Only trigger on enemy deaths
        if (deadPawn as EnemyMaster == null) return;
        
        // Only trigger on critical hits
        if (damageType != GameEvents.DamageType.Crit) return;
        
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;

        OnTrigger?.Invoke(this, deadPawn.transform);

        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform deadEnemyTransform)
    {
        GameEvents.instance.TriggerActionCard(card, deadEnemyTransform);
        if (deadEnemyTransform == null) return;

        lastActionTime = Time.time;
        
        // Heal the player for the inherited damage value
        PawnMaster player = PlayerController.instance;
        if (player != null)
        {
            GameEvents.instance.HealPawn(damage, player, gameObject, player.transform);
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)damage, (int)mana));
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        OnTrigger -= TriggerAction;
    }
}
