using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: 309
// name: Chip Lotto
// desc: When you break an object in the scene, you have a chance to trigger a lightning to Amount: 2 nearby enemies. Small chance to destroy self.
// Damage: 12
// Mana: 4

public class CardActionChipLotto : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;

    [Header("Chip Lotto Settings")]
    public float destroySelfChance = 0.2f; // 20% chance to destroy self
    public float retriggerChance = 0.2f; // 20% chance to retrigger lightning

    private float lastActionTime = -10f;
    private bool isSubscribed = false;

    public override void OnCardEnable()
    {
        if (!isSubscribed && GameEvents.instance != null)
        {
            GameEvents.instance.OnDestroyObject += HandleOnDestroyObject;
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
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
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
            GameEvents.instance.OnDestroyObject -= HandleOnDestroyObject;
            isSubscribed = false;
        }
        OnTrigger -= TriggerAction;
    }

    private void HandleOnDestroyObject(Transform obj, GunBullet bullet)
    {
        if (CardManager.instance == null) return;
        if (Time.time - lastActionTime < actionCooldown) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, obj);
        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (UnityEngine.Random.value < destroySelfChance)
        {
            // Small chance to destroy self
            if (CardManager.instance != null)
            {
                OnCardDestroyed();
                GameEvents.instance.CardDiscarded(this);
            }
            else
            {
                Debug.LogWarning("CardManager instance is null, cannot destroy card.");
            }
            return;
        }

        // Find up to Amount: 2 nearby enemies
        CombatManager.instance?.ShootLightningChain(
            target,
            _damage : damage, 
            _retriggerChance : retriggerChance,
            _maxChain : (int)amount
        );
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability, (int)amount, damage, (int)mana));
    }
}
