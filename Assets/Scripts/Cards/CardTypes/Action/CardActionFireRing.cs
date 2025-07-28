using System;
using UnityEngine;
using System.Collections.Generic;

// id: custom
// name: Fire Ring
// desc: When you dodge, Probability: 70% to burn enemies around you. Burn Damage: 2. Cost Mana: 3
public class CardActionFireRing : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Fire Ring Settings")]
    public GameObject burnAOEPrefab; // Assign BurnAOE prefab in inspector
    public float spawnRadius = 0.5f;
    public float aoeRadius = 2f;

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
        GameEvents.instance.TriggerActionCard(card, target);
        if (burnAOEPrefab == null || target == null) return;
        SpawnObjects(
            _prefab: burnAOEPrefab,
            _count: 1,
            _position: target.position,
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
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)damage, (int)mana));
    }
}
