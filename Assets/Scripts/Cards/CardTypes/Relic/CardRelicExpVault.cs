using System.Collections;
using UnityEngine;

public class CardRelicExpVault : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    public int expAmount = 1;

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
        EnemyMaster enemy = deadPawn as EnemyMaster;
        if (enemy == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (UnityEngine.Random.value > probability / 100f) return;

        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, enemy.transform);
    }

    public void TriggerAction(CardMaster card, Transform deadEnemyTransform)
    {
        GameEvents.instance.TriggerActionCard(card, deadEnemyTransform);
        if (deadEnemyTransform == null) return;
        if (CombatManager.instance == null) return;

        CombatManager.instance.SpawnDrop(CombatManager.DropItem.Exp, deadEnemyTransform, expAmount);
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, expAmount));
    }
}
