using System;
using UnityEngine;
using System.Collections.Generic;

// id: custom
// name: Wild Fire
// desc: When a burning enemy dies, Probability: 50% to explode and burn enemies around. Cost: Coin: 3
public class CardActionWildFire : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Wild Fire Settings")]
    public GameObject burnAOEPrefab; // Assign BurnAOE prefab in inspector
    public float spawnRadius = 0.5f;
    public float aoeRadius = 2f;

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
        var enemy = pawn as EnemyMaster;
        if (enemy == null) return;
        // Check if enemy has burn dot
        bool hasBurnDot = false;
        if (enemy.currentDots != null)
        {
            foreach (var dot in enemy.currentDots)
            {
                if (dot.type == EnemyMaster.DotType.Burn)
                {
                    hasBurnDot = true;
                    break;
                }
            }
        }
        if (!hasBurnDot) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!CoinCounter.CanCostCoin(-(int)coin)) return;
        OnTrigger?.Invoke(this, enemy.transform);
        GameEvents.instance.UpdateCoins(-(int)coin);
    }

    public void TriggerAction(CardMaster card, Transform enemyTransform)
    {
        if (burnAOEPrefab == null || enemyTransform == null) return;
        SpawnObjects(
            _prefab: burnAOEPrefab,
            _count: 1,
            _position: enemyTransform.position,
            _radius: spawnRadius,
            _modifyObject: (obj) =>
            {
                var aoe = obj.GetComponent<BurnAOE>();
                if (aoe != null)
                {
                    aoe.maxDamage = damage;
                    aoe.burnDamage = damage;
                    aoe.maxRadius = aoeRadius;
                    aoe.isPlayingFx = true;
                    aoe.targetTags = new List<string> { "Enemy" };
                }
            }
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability, (int)damage, (int)coin));
    }
}
