using System;
using System.Collections.Generic;
using UnityEngine;

// id: 335
// name: Power Bank
// desc: When you break some scene item, Probability: 50% to shoot a shock chain towards nearby enemies. Cost: Coin: 7
public class CardActionPowerBank : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.2f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Power Bank Settings")]
    public float lightningDamage = 5f;
    public float lightningRetriggerChance = 0.3f; // Default from CombatManager
    public int lightningMaxChain = 2; // Default from CombatManager
    public string[] targetTags = new string[] { "Enemy" }; // Default from CombatManager
    public List<EnemyMaster.DotType> dotTypes = new List<EnemyMaster.DotType> { EnemyMaster.DotType.Shock };
    public float dotProbability = 1f;

    private float lastActionTime = -10f;
    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject += HandleOnDestroyObject;
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
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
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
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnDestroyObject(Transform destroyedObject, GunBullet bullet)
    {
        if (destroyedObject == null) return;
        
        // Only trigger if player broke it (check if bullet owner is player)
        // If bullet is null, we assume it might not be player induced, or we can check other conditions.
        // The prompt says "When you break", implying player action.
        // Usually OnDestroyObject is called when a bullet hits a destructible.
        // We'll check if bullet exists and owner is player, or if bullet is null but we want to be generous?
        // Let's stick to the pattern of other cards: usually just check OnDestroyObject.
        // But to be precise "When you break", we should check if bullet owner is player.
        // However, other cards like CardActionArtillery just check OnDestroyObject.
        // We will assume OnDestroyObject is sufficient context or bullet check if available.
        
        /* 
           Ref: CardActionArtillery.cs
           private void HandleOnDestroyObject(Transform objectTransform, GunBullet bullet)
           {
               if (bullet == null || explodeBulletPrefab == null) return; 
               ...
           }
           It checks bullet != null.
        */

        if (bullet == null) return; // Assume needs bullet to be "you break"
        // Optionally check bullet.gun.owner == PlayerController.instance.gameObject if strict.
        // For now, we follow the pattern.

        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        
        // Check coin cost
        if (!CoinCounter.CanCostCoin(-(int)coin)) return;

        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, destroyedObject);
        GameEvents.instance.UpdateCoins(-(int)coin);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (target == null || CombatManager.instance == null) return;

        // Shoot shock chain
        CombatManager.instance.ShootLightningChain(
            origin: target,
            _damage: lightningDamage,
            _retriggerChance: lightningRetriggerChance,
            _maxChain: lightningMaxChain,
            _enemyTags: targetTags,
            dotTypes: dotTypes,
            dotProbability: dotProbability
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)coin));
    }
}
