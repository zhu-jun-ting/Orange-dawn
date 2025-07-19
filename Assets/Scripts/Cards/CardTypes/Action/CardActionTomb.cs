using System;
using UnityEngine;

// id: custom
// name: Tomb
// desc: When you direct kill an enemy, Probability: 20% to call a skeleton to help you. Cost Health: 4
// HealthCost: 4
public class CardActionTomb : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Tomb Settings")]
    public GameObject skeletonPrefab;
    public float skeletonLifetime = 10f;
    public float skeletonDamageModifier = 0.5f;
    public float skeletonHealthModifier = 1.5f;
    public float radius = 2f;

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
        if (pawn == null || !pawn.isEnemy) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100) return;
        if (PlayerController.instance == null) return;
        if (!HealthBar.CanCostHealth(-health)) return;

        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, pawn.transform);
        GameEvents.instance.UpdateHealth(-(int)health);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        SpawnPawns(
            _prefab: skeletonPrefab,
            _count: (int) amount,
            _position: target != null ? target.position : Vector3.zero,
            _radius: radius,
            _modifyObject: (obj) =>
            {
                NPCMaster npc = obj.GetComponent<NPCMaster>();
                if (npc != null)
                {
                    npc.maxHP = health * skeletonHealthModifier;
                    npc.damage = damage * skeletonDamageModifier;
                    Destroy(obj, skeletonLifetime);
                }
            }
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(card_description, probability, (int)amount, (int)health));
    }
}
