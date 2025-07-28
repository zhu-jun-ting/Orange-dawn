using System;
using UnityEngine;

// id: custom
// name: Poisoned Knife
// desc: When you kill an enemy with a crit hit, Probability: 60% to spawn a poison area around. Damage: 7. Cost Coin: 4
public class CardActionPoisonedKnife : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Poisoned Knife Settings")]
    public GameObject poisonAOEPrefab; // Assign PoisonAOE prefab in inspector
    public float poisonRadius = 2.5f;

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
        if (damageType != GameEvents.DamageType.Crit) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100) return;
        if (!CoinCounter.CanCostCoin(-(int)coin)) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, pawn.transform);
        GameEvents.instance.UpdateCoins(-(int)coin);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        SpawnObjects(
            _prefab: poisonAOEPrefab,
            _count: 1,
            _position: target != null ? target.position : Vector3.zero,
            _radius: poisonRadius,
            _modifyObject: (obj) =>
            {
                PoisonAOE aoe = obj.GetComponent<PoisonAOE>();
                if (aoe != null)
                {
                    aoe.maxDamage = damage;
                    aoe.maxRadius = poisonRadius;
                    aoe.isPlayingFx = true;
                }
            }
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)damage, (int)coin));
    }
}
