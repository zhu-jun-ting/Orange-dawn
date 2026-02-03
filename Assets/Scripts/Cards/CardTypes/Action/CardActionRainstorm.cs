using System;
using System.Collections.Generic;
using UnityEngine;

// id: 336
// name: Rainstorm
// desc: Died enemy will Probability: 10% to shock nearby enemies. Cost: Coin: 9
public class CardActionRainstorm : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.2f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Rainstorm Settings")]
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
            GameEvents.instance.OnPawnDie += HandleOnPawnDie;
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
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
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
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnPawnDie(PawnMaster pawn, float killDamage, GameObject instigator, GameEvents.DamageType damageType, Gun gun)
    {
        if (pawn == null || !pawn.isEnemy) return; // Only trigger on enemy death

        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        
        // Check coin cost
        if (!CoinCounter.CanCostCoin(-(int)coin)) return;

        lastActionTime = Time.time;
        // Trigger at the dead enemy's location
        OnTrigger?.Invoke(this, pawn.transform);
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
