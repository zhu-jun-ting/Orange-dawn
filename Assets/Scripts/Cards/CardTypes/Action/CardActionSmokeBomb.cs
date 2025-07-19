using System;
using UnityEngine;
using System.Collections.Generic;

// id: custom
// name: Smoke Bomb
// desc: When you dodge, Probability: 70% push enemies away around you with Damage: 6, each enemy hit recover Health: 1. Cost Mana: 3
public class CardActionSmokeBomb : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Smoke Bomb Settings")]
    public GameObject impulseAOECentrifugalPrefab; // Assign ImpulseAOECentrifugal prefab in inspector
    public float recoverHealth = 1f;
    public float spawnRadius = 2.5f;
    public float aoeRadius = 3f;

    private float lastActionTime = -10f;
    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnPlayerDodge += HandleOnPlayerDodge;
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
            GameEvents.instance.OnPlayerDodge -= HandleOnPlayerDodge;
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
            GameEvents.instance.OnPlayerDodge -= HandleOnPlayerDodge;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnPlayerDodge(Transform player)
    {
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, player);
        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        if (impulseAOECentrifugalPrefab == null || target == null) return;
        SpawnObjects(
            _prefab: impulseAOECentrifugalPrefab,
            _count: 1,
            _position: target.position,
            _radius: spawnRadius,
            _modifyObject: (obj) =>
            {
                var aoe = obj.GetComponent<ImpulseAOECentrifugal>();
                if (aoe != null)
                {
                    aoe.maxDamage = damage;
                    aoe.maxRadius = aoeRadius;
                    aoe.isPlayingFx = true;
                    aoe.targetTags = new List<string> { "Enemy" };
                    // Assign healing callback
                    aoe.onPawnDamaged = (pawn, dmg) => {
                        if (PlayerController.instance != null)
                        {
                            GameEvents.instance.HealPawn(recoverHealth, PlayerController.instance, PlayerController.instance.gameObject, obj.transform);
                        }
                    };
                }
            }
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability, (int)damage, (int)mana, (int)recoverHealth));
    }
}
