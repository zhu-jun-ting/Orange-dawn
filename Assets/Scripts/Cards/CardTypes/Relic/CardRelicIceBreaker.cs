using System.Collections;
using UnityEngine;

// id: 504
// name: Ice Breaker
// desc: Each time you take any damage, Probability: 5% to unlock a random slot.

public class CardRelicIceBreaker : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public event System.Action<CardMaster, Transform> OnTrigger;
    public float actionCooldown { get; set; } = 1f;

    private float lastActionTime = -10f;

    public override void OnCardEnable()
    {
        base.OnCardEnable();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn += HandleOnHitPawn;
        OnTrigger -= TriggerAction;
        OnTrigger += TriggerAction;
    }

    public override void OnCardDisable()
    {
        base.OnCardDisable();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        OnTrigger -= TriggerAction;
    }

    public override void Reset()
    {
        base.Reset();
        if (GameEvents.instance != null)
            GameEvents.instance.OnHitPawn -= HandleOnHitPawn;
        OnTrigger -= TriggerAction;
    }

    private void HandleOnHitPawn(float damage, PawnMaster receiver, GameObject instigator, GameEvents.DamageType damageType, Transform location, float hitBack, Gun source)
    {
        // Only trigger when the player takes damage
        if (receiver == null || !receiver.isPlayer) return;
        
        // Basic cooldown check
        if (Time.time - lastActionTime < actionCooldown) return;
        
        // Probability check (inherited probability)
        if (UnityEngine.Random.value > probability / 100f) return;

        lastActionTime = Time.time;
        // Invoke trigger (location can be the player's transform)
        OnTrigger?.Invoke(this, receiver.transform);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (BoardArea.instance == null) return;
        
        if (BoardArea.instance.ActivateRandomSlot())
        {
            SoundManager.PlaySFX("GetCard");
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability));
    }
}
