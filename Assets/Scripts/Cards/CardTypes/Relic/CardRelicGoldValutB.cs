using System.Collections;
using UnityEngine;

// id: 501
// name: Gold Vault B
// desc: When an enemy dies, Probability: 5% to drop an additional coin

public class CardRelicGoldValutB : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    public int coinAmount = 1;

    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie += HandleOnPawnDie;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
    }

    public override void OnCardDisable()
    {
        base.OnCardDisable();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        OnTrigger -= TriggerAction;
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnPawnDie -= HandleOnPawnDie;
        OnTrigger -= TriggerAction;
    }

    private void HandleOnPawnDie(PawnMaster deadPawn, float killDamage, GameObject instigator, GameEvents.DamageType damageType, Gun source)
    {
        // Check if the dead pawn is an enemy
        EnemyMaster enemy = deadPawn as EnemyMaster;
        if (enemy == null) return;

        if (Time.time - lastActionTime < actionCooldown) return;
        
        // roll using inherited probability (0-100)
        if (UnityEngine.Random.value > probability / 100f) return;

        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, enemy.transform);
    }

    public void TriggerAction(CardMaster card, Transform deadEnemyTransform)
    {
        GameEvents.instance.TriggerActionCard(card, deadEnemyTransform);
        if (deadEnemyTransform == null) return;

        if (CombatManager.instance != null)
        {
            CombatManager.instance.SpawnDrop(CombatManager.DropItem.Coin, deadEnemyTransform, coinAmount);
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, coinAmount));
    }
}
