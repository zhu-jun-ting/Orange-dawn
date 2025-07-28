using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// id: 308
// name: Green Lotto
// desc: When you break an object in the scene, you have a [probability]% chance to grow all linked cards. This card has the same chance to be destroyed when you are destroyed.
// Probability: parent field

public class CardActionGreenLotto : CardMaster, ICardAction
{
    [Header("ICardAction Settings")]
    public float _actionCooldown = 0.5f;
    public float actionCooldown { get => _actionCooldown; set => _actionCooldown = value; }
    public event Action<CardMaster, Transform> OnTrigger;


    // [Header("Green Lotto Settings")]
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
        if (UnityEngine.Random.value > probability / 100f) return;
        if (!ManaBar.CanCostMana(-(int)mana)) return;
        lastActionTime = Time.time;
        OnTrigger?.Invoke(this, obj);
        GameEvents.instance.UpdateMana(-(int)mana);
    }

    public void TriggerAction(CardMaster card, Transform target)
    {
        GameEvents.instance.TriggerActionCard(card, target);
        if (UnityEngine.Random.value > 0.5f)
        {
            // All action logic here: destroy the card
            if (CardManager.instance != null)
            {
                OnCardDestroyed();
                GameEvents.instance.CardDiscarded(this);
            }
            else
            {
                Debug.LogWarning("CardManager instance is null, cannot destroy card.");
            }
        }
        else
        {
            // All action logic here: grow all linked cards
            List<CardMaster> linkedCards = GetLinkedCards();
            foreach (var linked in linkedCards)
            {
                linked.Grow(1);
            }
        }
    }

    public override string GetDescription()
    {
        return GameSettings.AddIcon(string.Format(GameSettings.LocalizeText(card_description), probability));
    }
}
