using System;
using UnityEngine;

// id: 501
// name: Gold Vault
// desc: When an enemy dies, Probability: 5% to drop an additional coin

public class CardRelicGoldValut : CardMaster
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    private float lastActionTime = -10f;
    protected override void Awake()
    {
        base.Awake();
        // you can set a default probability in the inspector; leave inherited `probability` to be used
    }

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
            GameEvents.instance.OnPawnDie += HandleOnPawnDie;
        }
        
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
    }

    public override void OnCardDisable()
    {
        base.OnCardDisable();
        if (GameEvents.instance != null)
        {
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        }
        
        OnTrigger -= TriggerAction;
    }

    private void HandleOnPawnDie(PawnMaster deadPawn, float killDamage, GameObject instigator, GameEvents.DamageType damageType, Gun source)
    {
        // Only trigger for enemy deaths
        var enemy = deadPawn as EnemyMaster;
        if (enemy == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        // roll using inherited probability (0-100)
        float roll = UnityEngine.Random.Range(0f, 100f);
        if (roll > probability) return;

        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, enemy.transform);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (target == null) return;
        if (CombatManager.instance != null)
        {
            CombatManager.instance.SpawnDrop(CombatManager.DropItem.Coin, target, 1);
        }
    }

    public override bool OnCardDestroyed()
    {
        if (card_conditions != null && card_conditions.Contains(CardCondition.IsEternal)) return false;
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        return base.OnCardDestroyed();
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability));
    }
}