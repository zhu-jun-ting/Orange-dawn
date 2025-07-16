using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: 306
// name: Mass Effect
// desc: When accumulated DMG reaches 200 HP, explode around you and push enemies away (ImpulseAOECentrifugal)
// Damage: 50
// Probability: 30%

public class CardActionMassEffect : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Mass Effect Settings")]
    public GameObject impulseAOECentrifugalPrefab; // Assign in inspector
    public float explosionRadius = 2f;
    public float requiredAccumulatedDamage = 200f;
    public float explosionCentrifugalForce = 20f;

    private float lastActionTime = -10f;
    private float accumulatedDamage = 0f;

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

    private void HandleOnHitPawn(float dmg, PawnMaster reciever, GameObject instigator, GameEvents.DamageType dmgType, Transform location, float hitBack, Gun source)
    {
        if (impulseAOECentrifugalPrefab == null) return;
        Transform player = PlayerController.instance != null ? PlayerController.instance.transform : null;
        if (player == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        accumulatedDamage += dmg;
        if (accumulatedDamage < requiredAccumulatedDamage) return;
        accumulatedDamage = 0f;
        if (UnityEngine.Random.value > probability / 100f) return;
        lastActionTime = Time.time;
        

        // Spawn AOE at player position
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        OnTrigger?.Invoke(this, player);
        GameEvents.instance.UpdateMana(-(int)mana);
        
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        // Always write the actual effect here
        SpawnObjects(impulseAOECentrifugalPrefab, _count: 1, _position: target.position, _rotation: Quaternion.identity, _radius: 0.2f, _modifyObject: (obj) =>
        {
            var aoe = obj.GetComponent<ImpulseAOECentrifugal>();
            if (aoe != null)
            {
                aoe.maxDamage = damage;
                aoe.maxRadius = explosionRadius;
                aoe.centrifugalForce = explosionCentrifugalForce;
            }
        });
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, requiredAccumulatedDamage, explosionRadius, explosionCentrifugalForce));
    }

    public override void Reset()
    {
        base.Reset();
        accumulatedDamage = 0f;
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        OnTrigger -= TriggerAction;
    }
}
