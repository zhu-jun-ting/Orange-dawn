using System;
using System.Collections.Generic;
using UnityEngine;

// id: 333
// name: Humidity
// desc: When you heal yourself, Probability: 10% to shoot a lightning chain that deal Damage: 5 Cost: Mana: 5
public class CardActionHumidity : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Humidity Settings")]
    public float lightningRetriggerChance = 0.3f; // Default from CombatManager
    public int lightningMaxChain = 2; // Default from CombatManager
    public string[] targetTags = new string[] { "Enemy" }; // Default from CombatManager

    private float lastActionTime = -10f;
    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnHealPawn += HandleOnHealPawn;
            isSubscribed = true;
        }
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnHealPawn -= HandleOnHealPawn;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    public override void Reset()
    {
        base.Reset();
        if (isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnHealPawn -= HandleOnHealPawn;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnHealPawn(float healAmount, PawnMaster receiver, GameObject instigator, Transform location)
    {
        if (receiver == null || !receiver.isPlayer) return;
        if (healAmount <= 0) return; // Only trigger on positive healing
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        
        // Check mana cost
        if (!ManaBar.CanCostMana(-(int)mana)) return;

        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, receiver.transform);
        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (target == null || CombatManager.instance == null) return;

        // Shoot lightning chain
        CombatManager.instance.ShootLightningChain(
            origin: target,
            _damage: damage,
            _retriggerChance: lightningRetriggerChance,
            _maxChain: lightningMaxChain,
            _enemyTags: targetTags,
            dotTypes: null,
            dotProbability: 0f
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)damage, (int)mana));
    }
}
