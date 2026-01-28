using System;
using UnityEngine;
using System.Collections.Generic;

// id: custom
// name: Musketeers
// desc: When a burning enemy dies, Probability: 10% to Deal Damage: 7 Along a line. Cost: Health: 4
public class CardActionMusketeers : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Musketeers Settings")]
    public GameObject lightBeamStraightPrefab; // Assign LightBeamStraight prefab in inspector
    public float spawnRadius = 0.5f;

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
        if (!HealthBar.CanCostHealth(-(int)health)) return;
        OnTrigger?.Invoke(this, enemy.transform);
        GameEvents.instance.UpdateHealth(-(int)health);
    }

    public void TriggerAction(CardMaster card, Transform enemyTransform)
    {
        GameEvents.instance.TriggerActionCard(card, enemyTransform);
        if (lightBeamStraightPrefab == null || enemyTransform == null) return;
        SpawnObjects(
            _prefab: lightBeamStraightPrefab,
            _count: 1,
            _position: enemyTransform.position,
            _radius: spawnRadius,
            _modifyObject: (obj) =>
            {
                var beam = obj.GetComponent<LightBeamStraight>();
                if (beam != null)
                {
                    beam.damage = (int)damage;
                    beam.horizontal = true;
                }
            }
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)damage, (int)health));
    }
}
