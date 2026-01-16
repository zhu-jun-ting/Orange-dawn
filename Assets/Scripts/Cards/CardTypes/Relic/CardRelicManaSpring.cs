using System.Collections;
using UnityEngine;

public class CardRelicManaSpring : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    public int minManaRestore = 1;
    public int maxManaRestore = 5;

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
        if (deadEnemyTransform == null) return;
        if (GameEvents.instance == null) return;

        int minValue = Mathf.Min(minManaRestore, maxManaRestore);
        int maxValue = Mathf.Max(minManaRestore, maxManaRestore) + 1;
        int manaGain = UnityEngine.Random.Range(minValue, maxValue);
        GameEvents.instance.UpdateMana(manaGain);
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, minManaRestore, maxManaRestore));
    }
}
