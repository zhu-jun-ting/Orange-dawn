using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: 305
// name: Heavy Punch
// desc: When you deal a one time DMG exceeds Damage: 50, explode the exceeded damage (ImpulseAOE)
// Damage: 50

public class CardActionHeavyPunch : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Heavy Punch Settings")]
    public float damageThreshold = 50f; // Damage threshold to trigger explosion
    public GameObject impulseAOEPrefab; // Assign in inspector
    public float explosionRadius = 2f;
    public float explosionDelay = 0.1f;

    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn += HandleOnHitPawn;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
        base.OnCardEnable();
    }

    public override void OnCardDisable()
    {
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        OnTrigger -= TriggerAction;
        base.OnCardDisable();
    }

    private float exceeded = 0f; // Store the exceeded damage   
    private void HandleOnHitPawn(float dmg, PawnMaster reciever, GameObject instigator, GameEvents.DamageType dmgType, Transform location, float hitBack, Gun source)
    {
        if (impulseAOEPrefab == null || location == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (dmg <= damageThreshold) return;
        exceeded = dmg - damageThreshold;
        if (exceeded <= 0) return;
        lastActionTime = Time.time;

        // Spawn explosion at hit location using SpawnObjects
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        OnTrigger?.Invoke(this, location);
        GameEvents.instance.UpdateMana(-(int)mana);
        
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        SpawnObjects(impulseAOEPrefab, 1, target.position, Quaternion.identity, explosionRadius, (obj) =>
        {
            var aoe = obj.GetComponent<ImpulseAOE>();
            if (aoe != null)
            {
                aoe.maxDamage = exceeded;
                aoe.maxRadius = explosionRadius;
            }
        });
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, damageThreshold));
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        OnTrigger -= TriggerAction;
    }
}
